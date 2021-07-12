using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GameAI;

// All the Fuzz
using Tochas.FuzzyLogic;
using Tochas.FuzzyLogic.MembershipFunctions;
using Tochas.FuzzyLogic.Evaluators;
using Tochas.FuzzyLogic.Mergers;
using Tochas.FuzzyLogic.Defuzzers;
using Tochas.FuzzyLogic.Expressions;

namespace GameAICourse
{

    public class FuzzyVehicle : AIVehicle
    {

        // TODO create some Fuzzy Set enumeration types, and member variables for:
        // Fuzzy Sets (input and output), one or more Fuzzy Value Sets, and Fuzzy
        // Rule Sets for each output.
        // Also, create some methods to instantiate each of the member variables
        enum VehicleSpeed { Slow, Medium, Fast}
        enum VehiclePosition { Left, Center, Right}

        enum DesiredTurnRate { Left, Center, Right}

        enum DesiredSpeed { BrakeHard, Coast, Fast }

        FuzzySet<DesiredSpeed> desiredSpeed;
        FuzzySet<DesiredTurnRate> desiredTurnRate;
        FuzzySet<VehicleSpeed> currentSpeed;
        FuzzySet<VehiclePosition> currentPosition;
        FuzzyRuleSet<DesiredSpeed> throttleRuleSet;
        FuzzyRuleSet<DesiredTurnRate> steeringRuleSet;
        FuzzyValueSet inputs;
        FuzzyValueSet inputs2;

        private FuzzySet<VehicleSpeed> GetSpeedSet()
        {
            IMembershipFunction SlowFx = new ShoulderMembershipFunction(0f, new Coords(0f,1f), new Coords(30f,0f), 80f);
            IMembershipFunction MediumFx = new TriangularMembershipFunction(new Coords(30f, 0f), new Coords(50f,1f), new Coords(80f,0f));
            IMembershipFunction FastFx = new ShoulderMembershipFunction(0f, new Coords(50f,0f), new Coords(80f,1f), 80f);
            
            FuzzySet<VehicleSpeed> set = new FuzzySet<VehicleSpeed>();
            set.Set(new FuzzyVariable<VehicleSpeed>(VehicleSpeed.Slow, SlowFx));
            set.Set(new FuzzyVariable<VehicleSpeed>(VehicleSpeed.Medium, MediumFx));
            set.Set(new FuzzyVariable<VehicleSpeed>(VehicleSpeed.Fast, FastFx));
            return set;
        }        
        private FuzzySet<VehiclePosition> GetVehiclePositionSet()
        {
            IMembershipFunction LeftFx = new ShoulderMembershipFunction(-3f, new Coords(-3f, 1f), new Coords(-.01f, 0f),3f);
            IMembershipFunction CenterFx = new TriangularMembershipFunction(new Coords(-3f, 0f), new Coords(0f,1f), new Coords(3f, 0f));
            IMembershipFunction RightFx = new ShoulderMembershipFunction(-3f, new Coords(.01f, 0f), new Coords(3f, 1f), 3f);           
            
            FuzzySet<VehiclePosition> set = new FuzzySet<VehiclePosition>();
            set.Set(new FuzzyVariable<VehiclePosition>(VehiclePosition.Left, LeftFx));
            set.Set(new FuzzyVariable<VehiclePosition>(VehiclePosition.Center, CenterFx));
            set.Set(new FuzzyVariable<VehiclePosition>(VehiclePosition.Right, RightFx));
            return set;
        }

        private FuzzySet<DesiredSpeed> GetDesiredSpeedSet()
        {
            IMembershipFunction BrakeHardFx = new ShoulderMembershipFunction(-80, new Coords(-30, 1f), new Coords(-10, 0f), 80f);
            IMembershipFunction CoastFx = new TriangularMembershipFunction(new Coords(-80f, 0f), new Coords(0f, 1f), new Coords(80f,0f));
            IMembershipFunction FastFx = new ShoulderMembershipFunction(-80f, new Coords(50f, 0f), new Coords(80f, 1f), 80f);

            FuzzySet<DesiredSpeed> set = new FuzzySet<DesiredSpeed>();
            set.Set(new FuzzyVariable<DesiredSpeed>(DesiredSpeed.BrakeHard, BrakeHardFx));
            set.Set(new FuzzyVariable<DesiredSpeed>(DesiredSpeed.Coast, CoastFx));
            set.Set(new FuzzyVariable<DesiredSpeed>(DesiredSpeed.Fast, FastFx));
            return set;
        }

        private FuzzySet<DesiredTurnRate> GetDesiredTurnRateSet()
        {

            IMembershipFunction LeftFx = new ShoulderMembershipFunction(-.8f, new Coords(-.8f, 1f), new Coords(-.2f, 0f), .8f);
            IMembershipFunction CenterFx = new TriangularMembershipFunction(new Coords(-.8f, 0f), new Coords(0f, 1f), new Coords(.8f, 0f));
            IMembershipFunction RightFx = new ShoulderMembershipFunction(-.8f, new Coords(.2f, 0f), new Coords(.8f, 1f), .8f);
            

            FuzzySet<DesiredTurnRate> set = new FuzzySet<DesiredTurnRate>();
            set.Set(new FuzzyVariable<DesiredTurnRate>(DesiredTurnRate.Left, LeftFx));
            set.Set(new FuzzyVariable<DesiredTurnRate>(DesiredTurnRate.Center, CenterFx));
            set.Set(new FuzzyVariable<DesiredTurnRate>(DesiredTurnRate.Right, RightFx));
            return set;
        }

        private FuzzyRule<DesiredSpeed>[] GetThrottleRules()
        {
            FuzzyRule<DesiredSpeed>[] rules = new FuzzyRule<DesiredSpeed>[3];
            rules[0] = VehicleSpeed.Slow.Expr().Then(DesiredSpeed.Fast);
            rules[1] = VehicleSpeed.Medium.Expr().Then(DesiredSpeed.Coast);
            rules[2] = VehicleSpeed.Fast.Expr().Then(DesiredSpeed.Coast);

            /*rules[0] = VehicleSpeed.Slow.Expr().And(VehiclePosition.Center.Expr()).Then(DesiredSpeed.Fast);
            rules[1] = VehicleSpeed.Slow.Expr().And(VehiclePosition.Left.Expr()).Then(DesiredSpeed.Coast);
            rules[2] = VehicleSpeed.Slow.Expr().And(VehiclePosition.Right.Expr()).Then(DesiredSpeed.Coast);

            rules[3] = VehicleSpeed.Medium.Expr().And(VehiclePosition.Center.Expr()).Then(DesiredSpeed.Coast);
            rules[4] = VehicleSpeed.Medium.Expr().And(VehiclePosition.Left.Expr()).Then(DesiredSpeed.Coast);
            rules[5] = VehicleSpeed.Medium.Expr().And(VehiclePosition.Right.Expr()).Then(DesiredSpeed.Coast);
           
            rules[6] = VehicleSpeed.Fast.Expr().Then(DesiredSpeed.BrakeHard);*/
            return rules;
        }

        private FuzzyRule<DesiredTurnRate>[] GetSteeringRules()
        {
            FuzzyRule<DesiredTurnRate>[] rules = new FuzzyRule<DesiredTurnRate>[3];
            rules[0] = VehiclePosition.Left.Expr().Then(DesiredTurnRate.Right);
            rules[1] = VehiclePosition.Center.Expr().Then(DesiredTurnRate.Center);
            rules[2] = VehiclePosition.Right.Expr().Then(DesiredTurnRate.Left);
            return rules;
        }

        private FuzzyRuleSet<DesiredSpeed> GetThrottleRuleSet(FuzzySet<DesiredSpeed> desiredSpeed)
        {
            var rules = this.GetThrottleRules();
            return new FuzzyRuleSet<DesiredSpeed>(desiredSpeed, rules);
        }

        private FuzzyRuleSet<DesiredTurnRate> GetSteeringRuleSet(FuzzySet<DesiredTurnRate> desiredTurnRate)
        {
            var rules = this.GetSteeringRules();
            return new FuzzyRuleSet<DesiredTurnRate>(desiredTurnRate, rules);
        }

        protected override void Awake()
        {
            base.Awake();

            StudentName = "Andrew Hinson";
            // Only the AI can control. No humans allowed!
            IsPlayer = true;

            // TODO: You can initialize a bunch of Fuzzy stuff here
            desiredSpeed = GetDesiredSpeedSet();
            desiredTurnRate = GetDesiredTurnRateSet();
            currentSpeed = GetSpeedSet();
            currentPosition = GetVehiclePositionSet();
            throttleRuleSet = GetThrottleRuleSet(desiredSpeed);
            steeringRuleSet = GetSteeringRuleSet(desiredTurnRate);

            inputs = new FuzzyValueSet();
            inputs2 = new FuzzyValueSet();

        }


        override protected void Update()
        {

            // TODO Do all your Fuzzy stuff here and pass the defuzzified values to 
            // the car like so:
            // Throttle = someValue; //[-1f, 1f] -1 is full brake, 0 is neutral, 1 is full throttle
            // Steering = someValue; // [-1f, 1f] -1 if full left, 0 is neutral, 1 is full right
            Vector3 difference = (transform.position - pathTracker.closestPointOnPath);
            float distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(pathTracker.closestPointOnPath.x, pathTracker.closestPointOnPath.z));
            float signed_angle = Vector3.SignedAngle(difference, pathTracker.closestPointDirectionOnPath, Vector3.up);
            //Debug.Log("DIR: " + signed_angle);
            //Debug.Log("DISTANCE: " + distance);
           /* Vector3 x = pathTracker.pathCreator.path.localPoints[pathTracker.currentClosestPathPointIndex];
            Vector3 y = pathTracker.pathCreator.path.localPoints[pathTracker.currentClosestPathPointIndex + 3];
            Debug.Log("FOWARD: " + transform.forward);
            Debug.Log("CLOSEST DIR: " + pathTracker.closestPointDirectionOnPath);*/
            distance = Vector3.Distance(transform.forward, pathTracker.closestPointDirectionOnPath);
            Debug.Log("DISTANCE: " + Vector3.Distance(transform.forward, pathTracker.closestPointDirectionOnPath));
            //Debug.Log("current: " + x);
            //Debug.Log("next: " + y);
            // EVAL THROTTLE
            float val = signed_angle > 0 ? distance * -1 : distance;
            currentPosition.Evaluate(val, inputs);            
            currentSpeed.Evaluate(Speed,inputs);
            var results = throttleRuleSet.Evaluate(inputs);
            float crisp = results / 80;
            //Debug.Log("THROTTLE: " + crisp);
            Throttle = crisp;

            // EVAL STEERING
            currentPosition.Evaluate(val*6f, inputs2);
            var results2 = steeringRuleSet.Evaluate(inputs2);
            Debug.Log("STEERING: " + results2);
            Steering = results2*1f;
           
            // recommend you keep the base call at the end, after all your FuzzyVehicle code so that
            // control inputs can be processed properly (Throttle, Steering)
            base.Update();
        }

    }
}

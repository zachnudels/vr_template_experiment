using Unity.Mathematics;

using UnityEngine;
using PBL.Types;

namespace ActionSimilarity
{

    
    public class FixationRotator
    {
        private float currentOrbit;
        private bool finished;
        private TurnDirection direction;

        private float turnTime;
        private float degPerFrame;
        private float remainingOrbit;

        private float elapsedTime;
        
        public TurnDirection Direction
        {
            get => direction;
            set => direction = value;
        }
        
        public float TurnTime
        {
            get => turnTime;
            set => turnTime = value;
        }

        public FixationRotator(float turnTime = 2.5f, TurnDirection direction = TurnDirection.Right)
        { 
        /*
           * The issue here is that we need a constant acceleration,
           * that's why Babak did the whole remainingOrbit thing
           * degPerFrame is like our constant acceleration
        */
           Reset(direction);
           this.turnTime = turnTime;
           degPerFrame = (180.0f / this.turnTime) / 90.0f;  // 90 FPS
        }

        public string LogString()
        {
            return $"remaining: {remainingOrbit}, current: {currentOrbit}";
        }

        public void Reset(TurnDirection direction)
        {
            this.direction = direction; 
            currentOrbit = 0.0f;
            finished = false;
            remainingOrbit = 180.0f;
            elapsedTime = 0f;
        }

        public void Step(float deltaTime)
        {
            if (finished) return;

            elapsedTime += deltaTime;
            float T     = turnTime;
            float halfT = T * 0.5f;
            float a     = 720f / (T * T);        // deg/s²
            float sign = -(int)direction;

            // piecewise linear velocity profile
            float v = (elapsedTime <= halfT)
                ? a * elapsedTime            // accelerate
                : a * (T - elapsedTime);     // decelerate

            // integrate to get angle increment
            float deltaAngle = v * deltaTime * sign;
            currentOrbit    += deltaAngle;

            if (elapsedTime >= T)
            {
                finished = true;
            }
        }
        
        public Vector3 GetCurrentPosition(float fixationDepth, Vector3 fixationPosition, FaceDirection faceDirection)
        {
            Vector2 xz = calculateOrbit(currentOrbit, fixationDepth*(int)faceDirection, Vector2.zero);
            return new Vector3(xz.x, fixationPosition.y, xz.y);
        }
        
        public Vector2 calculateOrbit(float currentOrbitDegrees, float distanceFromCenterPoint, Vector2 centerPoint)
         {
             float radians = Mathf.Deg2Rad * (currentOrbitDegrees + 90);

             float x = (Mathf.Cos(radians) * distanceFromCenterPoint) + centerPoint.x;
             float y = (Mathf.Sin(radians) * distanceFromCenterPoint) + centerPoint.y;

             return new Vector2(x, y);
         }

        public bool isFinished()
        {
            return this.finished;
        }
    }
}
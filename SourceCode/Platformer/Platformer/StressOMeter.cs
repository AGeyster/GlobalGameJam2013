using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platformer
{
    public class StressOMeter
    {
        private const double deathState = 220;
        private const double startingHeartState = 100;
        private const double lowestHeartState = 80;
        private const double enemyHeartStateIncrease = 3;
        private const double runHeartIncrease = .1;
        private const double enemyKillDecrease = -10;
        private const double idleDecrease = -.1;
        private const double jumpIncrease = .5;
        private double currentHeartRate;
        public StressOMeter()
        {
            currentHeartRate = 100;
        }
        public double getCurrentHeartRate()
        {
            return currentHeartRate;
        }
        public void setCurrentHeartRate(double currentHeartRate)
        {
            this.currentHeartRate = currentHeartRate;
        }
        public void crouching()
        {
            if (currentHeartRate + (2 * idleDecrease) > lowestHeartState)
                currentHeartRate += (idleDecrease * 2);
            else
                currentHeartRate = lowestHeartState;
        }
        public void run()
        {
            if ((currentHeartRate +runHeartIncrease)< deathState)
                currentHeartRate += runHeartIncrease;
            else
                currentHeartRate = deathState;
        }
        public void idle()
        {
            if ((currentHeartRate + idleDecrease) > lowestHeartState)
                currentHeartRate += idleDecrease;
            else
                currentHeartRate = lowestHeartState;
        }
        public void enemyKill()
        {
            if ((currentHeartRate + enemyKillDecrease) > lowestHeartState)
                currentHeartRate += enemyKillDecrease;
            else
                currentHeartRate = lowestHeartState;
        }
        public void enemyDetect()
        {
            if ((currentHeartRate + enemyHeartStateIncrease) < deathState)
                currentHeartRate += enemyHeartStateIncrease;
            else
                currentHeartRate = deathState;
        }
        public bool isDead()
        {
            if (currentHeartRate == deathState)
                return true;
            else
                return false;
        }
        public void Jump()
        {
            if ((currentHeartRate + jumpIncrease) < deathState)
                currentHeartRate += jumpIncrease;
            else
                currentHeartRate = deathState;
        }
    }
}

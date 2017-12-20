﻿using System;

using UniCircleTools;
using UniCircleTools.Beatmaps;

namespace UniCircleDifficulty.Skills.Clicking
{
    /// <summary>
    /// Skill representing the difficulty of keeping up with tapping speed of notes
    /// </summary>
    class Speed : Skill
    {
        // Shortcuts for readability
        private ClickPoint ClickPointB => GetDifficultyPoint(1) as ClickPoint;
        private ClickPoint ClickPointA => GetDifficultyPoint(0) as ClickPoint;

        // Excertion decay rate
        protected override double ExcertionDecayBase => 0.3;

        protected override double SkillMultiplier => 5;

        public override void ProcessHitObject(HitObject hitObject)
        {
            if (hitObject is Spinner)
            {
                // Spinners are not considered in speed
                return;
            }

            // Construct diff points from hitobject and call ProcessDifficultyPoint with them
            throw new NotImplementedException();
        }

        protected override void UpdateDifficultyPoints(DifficultyPoint diffPoint)
        {
            // Add diffPoint to currentDiffPoints
            _currentDiffPoints.Add(diffPoint as ClickPoint);

            // Update pool
            if (_currentDiffPoints.Count == 2)
            {
                _currentDiffPoints.RemoveAt(0);
            }
        }

        protected override double CalculateRawDiff()
        {
            // In ppv2, higher spaced objects are worth more to reward spaced streams.
            // This can is really part of aim, and thus speed is not concerned with it.
            return 1.0 / (ClickPointA.Time - ClickPointB.Time);
        }

        protected override double CalculateBonusDiff()
        {
            // Accuracy difficulty
            return base.CalculateBonusDiff();
        }

        public Speed(Mods mods) : base(mods) { }
    }
}

﻿using System.Linq;
using System.Collections.Generic;

using UniCircleTools;
using UniCircleTools.Beatmaps;
using UniCircle.Difficulty.Skills;
using UniCircle.Difficulty.Standard;

namespace UniCircle.Score
{
    public enum HitJudgement
    {
        Hit300,
        Hit100,
        Hit50,
        Miss
    }

    public enum ScoreStyle
    {
        V1,
        V2
    }

    public class ScoreProcessor
    {
        public ScoreStyle ScoreStyle { get; }
        public double Score { get; private set; }
        public int Combo { get; set; }
        public int MaxCombo { get; set; }

        private List<ScoreObject> _remainingScoreObjects = new List<ScoreObject>();

        private Beatmap _beatmap;
        private Mods _mods;

        private const double AimingWeight = 0.4;
        private const double ClickingWeight = 0.4;
        private const double ReadingWeight = 0.2;
        private const double MaxScore = 1000000;

        private double _aimingPoints;
        private double _maxAimingPoints;
        private double _clickingPoints;
        private double _maxClickingPoints;
        private double _readingPoints;
        private double _maxReadingPoints;

        public ScoreProcessor(Beatmap beatmap, Mods mods, ScoreStyle scoreStyle)
        {
            ScoreStyle = scoreStyle;
            _beatmap = beatmap;
            _mods = mods;
            ProcessBeatmap();
        }

        public void ApplyHitJudgement(HitJudgement judgement)
        {
            ScoreObject currentObject = _remainingScoreObjects.FirstOrDefault();
            if (currentObject == null)
            {
                return;
            }
            _remainingScoreObjects.RemoveAt(0);

            if (ScoreStyle == ScoreStyle.V1)
            {
                double hitValue = AimingPoints(judgement) * currentObject.AimingDifficulty + ClickingPoints(judgement) * currentObject.ClickingDifficulty + ReadingPoints(judgement) * currentObject.ReadingDifficulty;

                // Mods multiplier is considered in difficulty multiplier
                Score += hitValue + hitValue * Combo / 100;
            }
            else if (ScoreStyle == ScoreStyle.V2)
            {
                //  aim => scales with hitrate (perhaps scale with max combo too?)
                //  click => scales with acc
                //  read => scales with acc
                _aimingPoints += AimingPoints(judgement) * currentObject.AimingDifficulty;
                _maxAimingPoints += AimingPoints(HitJudgement.Hit300) * currentObject.AimingDifficulty;
                _clickingPoints += ClickingPoints(judgement) * currentObject.ClickingDifficulty;
                _maxClickingPoints += ClickingPoints(HitJudgement.Hit300) * currentObject.ClickingDifficulty;
                _readingPoints += ReadingPoints(judgement) * currentObject.ReadingDifficulty;
                _maxReadingPoints += ReadingPoints(HitJudgement.Hit300) * currentObject.ReadingDifficulty;

                // where aim, click and read are portions max possible performance
                Score = MaxScore * ((_aimingPoints / _maxAimingPoints) * AimingWeight + (_clickingPoints / _maxClickingPoints) * ClickingWeight + (_readingPoints / _maxReadingPoints) * ReadingWeight);
            }

            Combo++;
            MaxCombo++;
        }

        private double AimingPoints(HitJudgement judgement)
        {
            switch (judgement)
            {
                case HitJudgement.Hit300:
                case HitJudgement.Hit100:
                case HitJudgement.Hit50:
                    return PointsForJudgement(HitJudgement.Hit300);
                default:
                    return 0;
            }
        }

        private double ClickingPoints(HitJudgement judgement)
        {
            return PointsForJudgement(judgement);
        }

        private double ReadingPoints(HitJudgement judgement)
        {
            return PointsForJudgement(judgement);
        }

        private double PointsForJudgement(HitJudgement judgement)
        {
            switch (judgement)
            {
                case HitJudgement.Hit300:
                    return 300;
                case HitJudgement.Hit100:
                    return 100;
                case HitJudgement.Hit50:
                    return 50;
                default:
                    return 0;
            }
        }

        private void ProcessBeatmap()
        {
            // In a lazer implementation, DifficultyPoints would be a property of HitObjects so
            //  there would be no need to recalculate the difficulty or do this messy sorting
            DifficultyCalculator calculator = new DifficultyCalculator();
            calculator.SetBeatmap(_beatmap);
            calculator.SetMods(_mods);
            calculator.CalculateDifficulty();
            
            foreach (DifficultyPoint diffPoint in calculator.Aiming.CalculatedPoints)
            {
                if (_remainingScoreObjects.Find(s => s.BaseObject == diffPoint.BaseObject) == null)
                {
                    _remainingScoreObjects.Add(new ScoreObject(diffPoint.BaseObject));
                }

                _remainingScoreObjects.Find(s => s.BaseObject == diffPoint.BaseObject).DifficultyPoints.Add(diffPoint);
            }

            foreach (DifficultyPoint diffPoint in calculator.Clicking.CalculatedPoints)
            {
                if (_remainingScoreObjects.Find(s => s.BaseObject == diffPoint.BaseObject) == null)
                {
                    _remainingScoreObjects.Add(new ScoreObject(diffPoint.BaseObject));
                }

                _remainingScoreObjects.Find(s => s.BaseObject == diffPoint.BaseObject).DifficultyPoints.Add(diffPoint);
            }

            foreach (DifficultyPoint diffPoint in calculator.Reading.CalculatedPoints)
            {
                if (_remainingScoreObjects.Find(s => s.BaseObject == diffPoint.BaseObject) == null)
                {
                    _remainingScoreObjects.Add(new ScoreObject(diffPoint.BaseObject));
                }

                _remainingScoreObjects.Find(s => s.BaseObject == diffPoint.BaseObject).DifficultyPoints.Add(diffPoint);
            }

            _remainingScoreObjects.OrderBy(s => s.BaseObject.Time);
        }
    }
}

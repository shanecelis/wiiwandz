﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WiiWandz.CloudBit;
using WiiWandz.Strokes;
using WiiWandz.Positions;

namespace WiiWandz.Spells
{
    abstract class CloudBitSpell : Spell
    {
		public String device;
		public String authorization;
        public int voltage;
        public int duration;

        public double confidence;

        protected double minConfidence;
        protected int minPercentOfTotalBetweenStartAndEndPoints;
        protected int maxPercentOfTotalBetweenStartAndEndPoints;
        protected List<StrokeDirection> acceptableDirectionsFromStartToEndPoint;

		protected CloudBitSignal cloudBit;
		protected List<List<StrokeDirection>> strokesForSpell;

		public DateTime lastTrigger;

        public CloudBitSpell(double confidence) 
        {
            this.confidence = confidence;
            this.acceptableDirectionsFromStartToEndPoint = new List<StrokeDirection>();
            this.minConfidence = 0.9999;
        }

        public CloudBitSpell(String device, String authorization, int voltage, int duration)
		{
            setConfigurations(device, authorization, voltage, duration);
			this.strokesForSpell = new List<List<StrokeDirection>>();
		}

        public void setConfigurations(String device, String authorization, int voltage, int duration)
        {
            this.device = device;
            this.authorization = authorization;
            this.voltage = voltage;
            this.duration = duration;
            this.cloudBit = new CloudBitSignal(this.device, this.authorization, voltage, duration * 1000);
        }

		public void castSpell()
		{
            if (cloudBit != null)
            {
                cloudBit.sendSignal();
            }
		}

		public Boolean casting()
		{
			return lastTrigger != null 
				&& (DateTime.Now.Subtract(lastTrigger).TotalMilliseconds < (this.duration * 1000));
		}

		public bool triggered(List<Position> positions)
		{
            bool trig = false;

            /*
            StrokeDecomposer decomposer = new StrokeDecomposer(PositionStatistics.MAX_X, PositionStatistics.MAX_Y, 10);
            List<Stroke> strokes = decomposer.determineStrokes(positions);
            List<StrokeDirection> directions = new List<StrokeDirection>();
            foreach (Stroke stroke in strokes)
            {
                directions.Add(stroke.direction);
            }

            foreach (List<StrokeDirection> strokeSet in this.strokesForSpell)
            {
                trig = trig || StrokeDecomposer.strokesMatch(directions, strokeSet);
            }
            */

            trig = verifyTrigger(positions);

            if (trig)
            {
                lastTrigger = DateTime.Now;
            }

            return trig;
        }

        protected virtual bool verifyTrigger(List<Position> positions)
        {
            PositionStatistics stats = new PositionStatistics(positions);

            StrokeDirection direction = StrokeDecomposer.determineDirection(stats.Start(), stats.End());
            double distance = stats.Diagonal();
            double relativeStartAndEndDistance = stats.FractionOfTotal(stats.Start(), stats.End());

            /*
            if (this.GetType() == typeof(Ascendio))
            {
                Console.WriteLine(
                    "Confidence: " + confidence 
                    + " direction: " + direction
                    + " (" + stats.Start().point.ToString() + " -> " + stats.End().point.ToString() + ")"
                    + " distance: " + distance + " (" + relativeStartAndEndDistance + ")");
            }
            */ 

            bool verified = false;

            foreach (StrokeDirection dir in acceptableDirectionsFromStartToEndPoint)
            {
                if (dir == direction)
                {
                    verified = true;
                }
            }

            verified = verified && confidence >= minConfidence;

            verified = verified && distance >= 400.0;
            verified = verified && (relativeStartAndEndDistance * 100) >= minPercentOfTotalBetweenStartAndEndPoints;
            verified = verified && (relativeStartAndEndDistance * 100) <= maxPercentOfTotalBetweenStartAndEndPoints;

            return verified;
        }

        public double getConfidence()
        {
            return this.confidence;
        }

    }
}

using System;


namespace Y_Track.YoutubeCaptureEngine.Models
{
    public class Range
    {
        public double Start { get; private set; }
        public double End { get; private set; }
        public double Length => (this.End - this.Start) + 1;

        public Range(double start , double end)
        {
            this.Start = start;
            this.End = end;
        }

        public Range(string rangeString)
        {
            if (rangeString == null) throw new ArgumentNullException();
            // if range don't contain (-) then it's the first packet and it starts with 0
            int indexOfDash = rangeString.IndexOf("-");
            if (indexOfDash == -1)
            {
                this.Start = 0;
                this.End = double.Parse(rangeString);
            }
            else
            {
                string[] startEndSeparated = rangeString.Split(new[] { "-" }, StringSplitOptions.None);
                this.Start = double.Parse(startEndSeparated[0]);
                this.End = double.Parse(startEndSeparated[1]);
            }
        }

        public override string ToString()
        {
            return "{ start : " + this.Start + "," + "end : " + this.End + "}";
        }

    }
}

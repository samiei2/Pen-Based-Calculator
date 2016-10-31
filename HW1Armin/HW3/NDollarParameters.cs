namespace HW1Armin.HW3
{
    internal class NDollarParameters
    {
        public static NDollarParameters Instance = new NDollarParameters();
        public bool RotationInvariant = false;
        public bool DoStartAngleComparison = false;
        public bool MatchOnlyIfSameNumberOfStrokes = false;
        public PossibleSearchMethods SearchMethod = PossibleSearchMethods.GSS;
        public double StartAngleThreshold = Utils.Deg2Rad(30.0);
        public bool TestFor1D = true;
        public bool UseUniformScaling = false;

        public enum PossibleSearchMethods { Protractor, GSS };
    }
}
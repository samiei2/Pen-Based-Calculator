using HW1Armin.HW3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Ink;

namespace HW1Armin
{
    internal class MachineRecognizer
    {
        NDollarRecognizer _nDollarRecog = new NDollarRecognizer();
        PennyPincher _pincher = new PennyPincher();
        public Dictionary<string,NBestList> Recognize(StrokeCollection strokeCollection)
        {
            Dictionary<string, NBestList> finalList = new Dictionary<string, NBestList>();
            // Sample 1
            NDollarParameters.Instance.SearchMethod = NDollarParameters.PossibleSearchMethods.GSS;
            NBestList nbestDollarN1 = _nDollarRecog.Recognize(strokeCollection);
            finalList.Add("ndollar", nbestDollarN1);
            NDollarParameters.Instance.SearchMethod = NDollarParameters.PossibleSearchMethods.Protractor;
            NBestList nbestProtractor1 = _nDollarRecog.Recognize(strokeCollection);
            finalList.Add("protractor", nbestProtractor1);
            NBestList nbestPincher1 = _pincher.Recognize(strokeCollection);
            finalList.Add("pincher", nbestPincher1);

            // Sample 3
            NDollarParameters.Instance.SearchMethod = NDollarParameters.PossibleSearchMethods.GSS;
            NBestList nbestDollarN3 = _nDollarRecog.Recognize(strokeCollection,3);
            finalList.Add("ndollar3", nbestDollarN3);
            NDollarParameters.Instance.SearchMethod = NDollarParameters.PossibleSearchMethods.Protractor;
            NBestList nbestProtractor3 = _nDollarRecog.Recognize(strokeCollection,3);
            finalList.Add("protractor3", nbestProtractor3);
            NBestList nbestPincher3 = _pincher.Recognize(strokeCollection,3);
            finalList.Add("pincher3", nbestPincher3);

            // Sample 5
            NDollarParameters.Instance.SearchMethod = NDollarParameters.PossibleSearchMethods.GSS;
            NBestList nbestDollarN5 = _nDollarRecog.Recognize(strokeCollection,5);
            finalList.Add("ndollar5", nbestDollarN5);
            NDollarParameters.Instance.SearchMethod = NDollarParameters.PossibleSearchMethods.Protractor;
            NBestList nbestProtractor5 = _nDollarRecog.Recognize(strokeCollection,5);
            finalList.Add("protractor5", nbestProtractor5);
            NBestList nbestPincher5 = _pincher.Recognize(strokeCollection,5);
            finalList.Add("pincher5", nbestPincher5);

            return finalList;
        }
    }
}
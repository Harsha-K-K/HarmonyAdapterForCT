using ChDefine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTHarmonyAdapters
{
    public class IncisiveModel
    {
        public ImageSeriesInfo imageSeriesInfo { get; private set; }

        public PatientInfo patientInfo { get; private set; }

        public StudyInfo studyInfo { get; private set; }

        public IncisiveModel(ImageSeriesInfo imageSeriesInfo, PatientInfo patientInfo, StudyInfo studyInfo)
        {
            this.imageSeriesInfo = imageSeriesInfo;
            this.patientInfo = patientInfo;
            this.studyInfo = studyInfo;
        }
    }
}

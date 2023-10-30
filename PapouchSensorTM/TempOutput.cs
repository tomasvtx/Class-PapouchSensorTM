namespace PapouchSensor
{
    public partial class PapouchSensorTM
    {
        /// <summary>
        /// Představuje výstup teplotního čtení obsahující teplotní hodnotu a případné chybové zprávy.
        /// </summary>
        public struct TempOutput
        {
            public float Temperature { get; set; }
            public string Error { get; set; }
        }
    }
}

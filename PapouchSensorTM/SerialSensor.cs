using System;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace PapouchSensor
{
    /// <summary>
    /// Tato třída představuje komunikační knihovnu pro "TM" sériový teploměr od PaPouch Electronic.
    /// </summary>
    public partial class PapouchSensorTM
    {
        /// <summary>
        /// Očekávaný formát odpovědi sériového senzoru
        /// </summary>
        private const string SerialDataFormat = @"^[\+\-]\d{3}\.\d{1}C";

        /// <summary>
        /// // Název sériového portu
        /// </summary>
        private string portName;

        /// <summary>
        /// Časový limit pro čtení v milisekundách
        /// </summary>
        private int readTimeout = 5000;  

        /// <summary>
        /// Získá nebo nastaví časový limit pro čtení sériové komunikace v milisekundách. Výchozí hodnota je 5000 (5 sekund).
        /// </summary>
        public int ReadTimeout
        {
            get { return readTimeout; }
            set
            {
                if (value < 1000)
                    throw new ArgumentException("ReadTimeout musí být alespoň 1000 milisekund.");
                readTimeout = value;
            }
        }

        /// <summary>
        /// Získá nebo nastaví název sériového portu, ze kterého se budou číst hodnoty (například "COM1").
        /// </summary>
        public string PortName
        {
            get { return portName; }
            set
            {
                if (string.IsNullOrEmpty(value) || !Regex.IsMatch(value, @"^COM\d+$"))
                    throw new ArgumentException("Název portu musí být ve formátu 'COMx', kde x je číslo.");
                portName = value;
            }
        }

        /// <summary>
        /// Inicializuje instanci této třídy.
        /// </summary>
        /// <param name="portName">Název sériového portu, ze kterého se budou číst hodnoty (například "COM1").</param>
        public PapouchSensorTM(string portName)
        {
            PortName = portName;
        }

        /// <summary>
        /// Čte aktuální teplotu.
        /// </summary>
        /// <param name="port">Instance SerialPort pro komunikaci.</param>
        /// <returns>Objekt TempOutput obsahující teplotu a případné chybové zprávy.</returns>
        public TempOutput ReadTemperature(ref SerialPort port)
        {
            try
            {
                // Zpoždění před zahájením komunikace
                Thread.Sleep(150);

                // Inicializace a konfigurace sériového portu
                port = new SerialPort(PortName, 9600, Parity.None, 8, StopBits.One);
                Thread.Sleep(150);
                port.ReadTimeout = ReadTimeout;
                port.NewLine = "\r"; // Carriage Return

                // Proměnná pro uchování přijatých dat
                string rawData = null;

                // Otevření sériového portu pro komunikaci
                port.Open();
                Thread.Sleep(150);

                // Vyprázdnění příchozího bufferu
                port.DiscardInBuffer();

                // Aktivace DTR (Data Terminal Ready) na sériovém portu
                port.DtrEnable = true;
                Thread.Sleep(150);

                // Čtení dat ze senzoru
                rawData = port.ReadLine();

                // Analýza a ověření formátu přijatých dat
                if (!Regex.IsMatch(rawData, SerialDataFormat))
                    throw new FormatException($"Senzor vrátil data ve neočekávaném formátu: '{rawData}'");

                // Úspěšné přečtení teploty a vrácení výsledků
                return new TempOutput
                {
                    Temperature = float.Parse(rawData.Substring(0, 6), System.Globalization.CultureInfo.InvariantCulture),
                    Error = string.Empty
                };
            }
            catch (Exception ex)
            {
                // Chyba při komunikaci se senzorem, vrácení chybového stavu
                return new TempOutput { Error = ex.Message };
            }
            finally
            {
                // Ukončení komunikace se senzorem a úklid
                Thread.Sleep(150);

                // Zavření sériového portu, pokud byl otevřen
                port?.Close();

                Thread.Sleep(150);

                // Uvolnění prostředků spojených se sériovým portem
                port?.Dispose();

                Thread.Sleep(150);
            }

        }
    }
}

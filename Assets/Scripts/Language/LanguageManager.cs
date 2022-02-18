using System.Globalization;
using System.Collections.Generic;
using UnityEngine;

namespace Language
{
    public static class LanguageManager
    {
        private static Dictionary<SystemLanguage, Dictionary<string, string>> langText = InitTextDB();
        private static Dictionary<SystemLanguage, CultureInfo> langInfo;
        private static SystemLanguage lang = GetDefaultLanguage();

        private static SystemLanguage GetDefaultLanguage()
        {
            SystemLanguage lang = Application.systemLanguage;

            if (PlayerPrefs.HasKey("langid"))
            {
                try
                {
                    lang = (SystemLanguage)PlayerPrefs.GetInt("langid");
                }
                catch (System.Exception)
                {
                    lang = Application.systemLanguage;
                }
            }

            if (!langText.ContainsKey(lang))
            {
                lang = SystemLanguage.English;
            }

            return lang;
        }

        public static string ToLangString(float f)
        {
            return f.ToString(langInfo[lang]);
        }

        public static SystemLanguage GetLanguage()
        {
            return lang;
        }

        public static void SetLanguage(SystemLanguage lang)
        {
            if (!langText.ContainsKey(lang))
                return;

            LanguageManager.lang = lang;
            PlayerPrefs.SetInt("langid", (int)lang);
        }

        public static string GetText(LangKey key, string token = null)
        {
            if (key == LangKey.None)
                return "";

            return GetText(key.ToString(), token);
        }

        public static string GetText(string key, string token = "")
        {
          //  Debug.Log("Get Text " + key);

            string text = " { " + key + " } ";

            if (null == key || ("").Equals(key) || !langText[lang].ContainsKey(key.ToLower()))
                return text;

            key = key.ToLower();

            if (langText[lang].ContainsKey(key))
                text = langText[lang][key];

            text = string.Format(text, token);
            return text.Trim();
        }

        private static Dictionary<SystemLanguage, Dictionary<string, string>> InitTextDB()
        {
            langInfo = new Dictionary<SystemLanguage, CultureInfo>();
            Dictionary<SystemLanguage, Dictionary<string, string>> textDB = new Dictionary<SystemLanguage, Dictionary<string, string>>();
            Dictionary<string, string> texts = new Dictionary<string, string>();
            textDB.Add(SystemLanguage.German, texts);
            langInfo.Add(SystemLanguage.German, new CultureInfo("de-DE"));

            texts.Add(SystemLanguage.English.ToString().ToLower(), "Englisch");
            texts.Add(SystemLanguage.German.ToString().ToLower(), "Deutsch");

            texts.Add(LangKey.Off.ToString().ToLower(), "{0} ist ausgeschaltet");
            texts.Add(LangKey.On.ToString().ToLower(), "{0} ist eingeschaltet");
            texts.Add(LangKey.Charging.ToString().ToLower(), "{0} lädt auf");
            texts.Add(LangKey.Resting.ToString().ToLower(), "{0} befindet sich im Ruhezustand");
            texts.Add(LangKey.Starting.ToString().ToLower(), "{0} startet");
            texts.Add(LangKey.Working.ToString().ToLower(), "{0} arbeitet");
            texts.Add(LangKey.Returning.ToString().ToLower(), "{0} kehrt zurück");

            texts.Add(LangKey.GoHere.ToString().ToLower(), "Gehe hierher");
            texts.Add(LangKey.Goto.ToString().ToLower(), "Gehe zu {0}");
            texts.Add(LangKey.PointerAction.ToString().ToLower(), "{0}");
            texts.Add(LangKey.LookAction.ToString().ToLower(), "Betrachte {0}");
            texts.Add(LangKey.GrabAction.ToString().ToLower(), "Nehme {0}");
            texts.Add(LangKey.DropAction.ToString().ToLower(), "Lege {0} ab");
            texts.Add(LangKey.UseAction.ToString().ToLower(), "Benutze {0}");
            texts.Add(LangKey.TalkAction.ToString().ToLower(), "Rede mit {0}");
            texts.Add(LangKey.OpenAction.ToString().ToLower(), "Öffne {0}");
            texts.Add(LangKey.CloseAction.ToString().ToLower(), "Schließe {0}");
            texts.Add(LangKey.Open.ToString().ToLower(), "Öffne {0}");
            texts.Add(LangKey.Close.ToString().ToLower(), "Schließe {0}");
            texts.Add(LangKey.Deactivate.ToString().ToLower(), "{0} deaktivieren");
            texts.Add(LangKey.Set.ToString().ToLower(), "{0} setzen");
            texts.Add(LangKey.Activate.ToString().ToLower(), "{0} aktivieren");
            texts.Add(LangKey.Delete.ToString().ToLower(), "{0} löschen");
            texts.Add(LangKey.Add.ToString().ToLower(), "{0} hinzufügen");
            texts.Add(LangKey.Show.ToString().ToLower(), "Zeige {0} an");
            texts.Add(LangKey.Hide.ToString().ToLower(), "Blende {0} aus");
            texts.Add(LangKey.NotAvailable.ToString().ToLower(), "{0} nicht vorhanden");
            texts.Add(LangKey.Saved.ToString().ToLower(), "{0} gespeichert");
            texts.Add(LangKey.Save.ToString().ToLower(), "{0} speichern");
            texts.Add(LangKey.Develop.ToString().ToLower(), "{0} entwickeln");
            texts.Add(LangKey.NotSaved.ToString().ToLower(), "{0} nicht gespeichert");
            texts.Add(LangKey.SwitchOff.ToString().ToLower(), "{0} ausschalten");
            
            texts.Add(LangKey.SwitchPlayer.ToString().ToLower(), "Spieler wechseln");

            texts.Add(LangKey.EG.ToString().ToLower(), "EG");
            texts.Add(LangKey.UG.ToString().ToLower(), "UG");
            texts.Add(LangKey.OG.ToString().ToLower(), "OG");

            texts.Add(LangKey.TV.ToString().ToLower(), "Fernseher");
            texts.Add(LangKey.PC.ToString().ToLower(), "Rechner");
            texts.Add(LangKey.AntivirusApp.ToString().ToLower(), "Antivirus");
            texts.Add(LangKey.TaskMgrApp.ToString().ToLower(), "Prozesse");
            texts.Add(LangKey.Programs.ToString().ToLower(), "Programme");
            texts.Add(LangKey.Malware.ToString().ToLower(), "Schadsoftware");
            texts.Add(LangKey.CalcApp.ToString().ToLower(), "Taschenrechner");
            texts.Add(LangKey.PromptApp.ToString().ToLower(), "Eingabeaufforderung");
            texts.Add(LangKey.TicTacToeApp.ToString().ToLower(), "Dodelschach");
            texts.Add(LangKey.Drawn.ToString().ToLower(), "Patt");
            texts.Add(LangKey.YouWin.ToString().ToLower(), "Sie gewinnen!");
            texts.Add(LangKey.YouLose.ToString().ToLower(), "Sie verlieren!");
            texts.Add(LangKey.NewGame.ToString().ToLower(), "Neues Spiel");
            texts.Add(LangKey.UnknownCommand.ToString().ToLower(), "Ungültiger Befehl {0}");
            texts.Add(LangKey.Portal.ToString().ToLower(), "Portal");
            texts.Add(LangKey.Window.ToString().ToLower(), "Fenster");

            texts.Add(LangKey.Car.ToString().ToLower(), "Auto {0}");
            texts.Add(LangKey.Lamp.ToString().ToLower(), "Lampe");
            texts.Add(LangKey.Mirror.ToString().ToLower(), "Spiegel");
            texts.Add(LangKey.Chair.ToString().ToLower(), "Stuhl");
            texts.Add(LangKey.Table.ToString().ToLower(), "Tisch");
            texts.Add(LangKey.ClosetDoor.ToString().ToLower(), "Schranktür");
            texts.Add(LangKey.Drawer.ToString().ToLower(), "Schublade");
            texts.Add(LangKey.Box.ToString().ToLower(), "Karton");
            texts.Add(LangKey.AreaMap.ToString().ToLower(), "Umgebungskarte");
            texts.Add(LangKey.ColorGrading.ToString().ToLower(), "Farbkorrektur");
            texts.Add(LangKey.Back.ToString().ToLower(), "Zurück");
            texts.Add(LangKey.TakePhoto.ToString().ToLower(), "Foto shießen");
            texts.Add(LangKey.Map.ToString().ToLower(), "Karte");
            texts.Add(LangKey.With.ToString().ToLower(), "mit");
            texts.Add(LangKey.Phone.ToString().ToLower(), "Telefon");
            texts.Add(LangKey.Key.ToString().ToLower(), "Schlüssel");
            texts.Add(LangKey.Handle.ToString().ToLower(), "Griff");
            texts.Add(LangKey.DoorBell.ToString().ToLower(), "Türklingel");
            texts.Add(LangKey.Door.ToString().ToLower(), "Tür");
            texts.Add(LangKey.FenceDoor.ToString().ToLower(), "Tor");
            texts.Add(LangKey.ElevatorButton.ToString().ToLower(), "Aufzugtaste");
            texts.Add(LangKey.ElevatorBellButton.ToString().ToLower(), "Nottaste");
            texts.Add(LangKey.Inventorybox.ToString().ToLower(), "Inventar");
            texts.Add(LangKey.Switch.ToString().ToLower(), "Schalter");
            texts.Add(LangKey.Cupboard.ToString().ToLower(), "Schrank");
            texts.Add(LangKey.Cabinette.ToString().ToLower(), "Schränkchen");
            texts.Add(LangKey.Robovac.ToString().ToLower(), "Staubsaugroboter");
            texts.Add(LangKey.Camera.ToString().ToLower(), "Kamera");
            texts.Add(LangKey.Image.ToString().ToLower(), "Bild {0}");
            texts.Add(LangKey.Error.ToString().ToLower(), "Fehler {0}");
            texts.Add(LangKey.Lid.ToString().ToLower(), "Deckel");
            texts.Add(LangKey.System.ToString().ToLower(), "System");
            texts.Add(LangKey.Clock.ToString().ToLower(), "Uhr");
            texts.Add(LangKey.CurrentTime.ToString().ToLower(), "Aktuelle Zeit");
            texts.Add(LangKey.Alarms.ToString().ToLower(), "Alarmzeiten");
            texts.Add(LangKey.Alarm.ToString().ToLower(), "Alarm");
            texts.Add(LangKey.Stopp.ToString().ToLower(), "Stoppe {0}");
            texts.Add(LangKey.AlarmClock.ToString().ToLower(), "Wecker");
            texts.Add(LangKey.CuckooClock.ToString().ToLower(), "Kuckucksuhr");
            texts.Add(LangKey.KillProcess.ToString().ToLower(), "Prozess beenden");
            texts.Add(LangKey.ScanPC.ToString().ToLower(), "PC scannen");
            texts.Add(LangKey.CleanPC.ToString().ToLower(), "PC bereinigen");
            texts.Add(LangKey.ScanReport.ToString().ToLower(), "Scan Report");
            texts.Add(LangKey.ScanResult.ToString().ToLower(), "Infizierte Objekte gefunden: {0}");
            texts.Add(LangKey.ObjectsFound.ToString().ToLower(), "{0} Objekt(e) gefunden");
            texts.Add(LangKey.ObjectsRemoved.ToString().ToLower(), "{0} Objekt(e) entfernt");


            texts = new Dictionary<string, string>();
            textDB.Add(SystemLanguage.English, texts);
            langInfo.Add(SystemLanguage.English, new CultureInfo("en-US"));

            texts.Add(SystemLanguage.English.ToString().ToLower(), "English");
            texts.Add(SystemLanguage.German.ToString().ToLower(), "German");

            texts.Add(LangKey.Off.ToString().ToLower(), "{0} is off");
            texts.Add(LangKey.On.ToString().ToLower(), "{0} is on");
            texts.Add(LangKey.Charging.ToString().ToLower(), "{0} is charging");
            texts.Add(LangKey.Resting.ToString().ToLower(), "{0} is resting");
            texts.Add(LangKey.Starting.ToString().ToLower(), "{0} is starting");
            texts.Add(LangKey.Working.ToString().ToLower(), "{0} is working");
            texts.Add(LangKey.Returning.ToString().ToLower(), "{0} is returning");
            texts.Add(LangKey.GoHere.ToString().ToLower(), "Go here");
            texts.Add(LangKey.Goto.ToString().ToLower(), "Go to {0}");
            texts.Add(LangKey.PointerAction.ToString().ToLower(), "{0}");
            texts.Add(LangKey.LookAction.ToString().ToLower(), "Look at {0}");
            texts.Add(LangKey.GrabAction.ToString().ToLower(), "Get {0}");
            texts.Add(LangKey.DropAction.ToString().ToLower(), "Drop {0}");
            texts.Add(LangKey.UseAction.ToString().ToLower(), "Use {0}");
            texts.Add(LangKey.TalkAction.ToString().ToLower(), "Talk to {0}");
            texts.Add(LangKey.OpenAction.ToString().ToLower(), "Open {0}");
            texts.Add(LangKey.CloseAction.ToString().ToLower(), "Open {0}");
            texts.Add(LangKey.Open.ToString().ToLower(), "Open {0}");
            texts.Add(LangKey.Close.ToString().ToLower(), "Close {0}");
            texts.Add(LangKey.Delete.ToString().ToLower(), "Delete {0}");
            texts.Add(LangKey.Set.ToString().ToLower(), "Set {0}");
            texts.Add(LangKey.Deactivate.ToString().ToLower(), "Deactivate {0}");
            texts.Add(LangKey.Activate.ToString().ToLower(), "Activate {0}");
            texts.Add(LangKey.Show.ToString().ToLower(), "Show {0}");
            texts.Add(LangKey.Hide.ToString().ToLower(), "Hide {0}");
            texts.Add(LangKey.Add.ToString().ToLower(), "Add {0}");
            texts.Add(LangKey.NotAvailable.ToString().ToLower(), "{0} not available");
            texts.Add(LangKey.Saved.ToString().ToLower(), "{0} saved");
            texts.Add(LangKey.Save.ToString().ToLower(), "Save {0}");
            texts.Add(LangKey.NotSaved.ToString().ToLower(), "{0} not saved");
            texts.Add(LangKey.Develop.ToString().ToLower(), "Develop {0}");
            texts.Add(LangKey.SwitchOff.ToString().ToLower(), "Switch off {0}");
            
            texts.Add(LangKey.SwitchPlayer.ToString().ToLower(), "Switch Player");

            texts.Add(LangKey.EG.ToString().ToLower(), "G");
            texts.Add(LangKey.UG.ToString().ToLower(), "B");
            texts.Add(LangKey.OG.ToString().ToLower(), "");

            texts.Add(LangKey.TV.ToString().ToLower(), "TV");
            texts.Add(LangKey.PC.ToString().ToLower(), "Computer");
            texts.Add(LangKey.AntivirusApp.ToString().ToLower(), "Antivirus");
            texts.Add(LangKey.TaskMgrApp.ToString().ToLower(), "Task Manager");
            texts.Add(LangKey.CalcApp.ToString().ToLower(), "Calculator");
            texts.Add(LangKey.PromptApp.ToString().ToLower(), "Prompt");
            texts.Add(LangKey.TicTacToeApp.ToString().ToLower(), "Tic Tac Toe");
            texts.Add(LangKey.Drawn.ToString().ToLower(), "Drawn");
            texts.Add(LangKey.YouWin.ToString().ToLower(), "You made it!");
            texts.Add(LangKey.YouLose.ToString().ToLower(), "You lose!");
            texts.Add(LangKey.NewGame.ToString().ToLower(), "New Game");
            texts.Add(LangKey.UnknownCommand.ToString().ToLower(), "Unknown Command {0}");
            texts.Add(LangKey.Portal.ToString().ToLower(), "Portal");
            texts.Add(LangKey.Window.ToString().ToLower(), "Window");

            texts.Add(LangKey.Car.ToString().ToLower(), "Car {0}");
            texts.Add(LangKey.Lamp.ToString().ToLower(), "Lamp");
            texts.Add(LangKey.Mirror.ToString().ToLower(), "Mirror");
            texts.Add(LangKey.Chair.ToString().ToLower(), "Chair");
            texts.Add(LangKey.Table.ToString().ToLower(), "Table");
            texts.Add(LangKey.AreaMap.ToString().ToLower(), "Area Map");
            texts.Add(LangKey.ColorGrading.ToString().ToLower(), "Color Grading");
            texts.Add(LangKey.Back.ToString().ToLower(), "Back");
            texts.Add(LangKey.TakePhoto.ToString().ToLower(), "Take Photo");
            texts.Add(LangKey.Map.ToString().ToLower(), "Map");
            texts.Add(LangKey.With.ToString().ToLower(), "with");
            texts.Add(LangKey.Phone.ToString().ToLower(), "Phone");
            texts.Add(LangKey.Key.ToString().ToLower(), "Key");
            texts.Add(LangKey.DoorBell.ToString().ToLower(), "Door Bell");
            texts.Add(LangKey.Door.ToString().ToLower(), "Door");
            texts.Add(LangKey.Handle.ToString().ToLower(), "Handle");
            texts.Add(LangKey.FenceDoor.ToString().ToLower(), "Fence Door");
            texts.Add(LangKey.ElevatorButton.ToString().ToLower(), "Elevator Button");
            texts.Add(LangKey.ElevatorBellButton.ToString().ToLower(), "Emergency Button");
            texts.Add(LangKey.Inventorybox.ToString().ToLower(), "Inventory");
            texts.Add(LangKey.Switch.ToString().ToLower(), "Switch");
            texts.Add(LangKey.Cupboard.ToString().ToLower(), "Cupboard");
            texts.Add(LangKey.Cabinette.ToString().ToLower(), "Cabinette");
            texts.Add(LangKey.ClosetDoor.ToString().ToLower(), "Closet Door");
            texts.Add(LangKey.Drawer.ToString().ToLower(), "Drawer");
            texts.Add(LangKey.Robovac.ToString().ToLower(), "Robovac");
            texts.Add(LangKey.Box.ToString().ToLower(), "Box");
            texts.Add(LangKey.Camera.ToString().ToLower(), "Camera");
            texts.Add(LangKey.Error.ToString().ToLower(), "Error {0}");
            texts.Add(LangKey.Image.ToString().ToLower(), "Image");
            texts.Add(LangKey.Lid.ToString().ToLower(), "Lid");
            texts.Add(LangKey.Clock.ToString().ToLower(), "Clock");
            texts.Add(LangKey.System.ToString().ToLower(), "System");
            texts.Add(LangKey.CurrentTime.ToString().ToLower(), "current time");
            texts.Add(LangKey.Alarms.ToString().ToLower(), "Alarm Times");
            texts.Add(LangKey.Alarm.ToString().ToLower(), "Alarm");
            texts.Add(LangKey.Stopp.ToString().ToLower(), "Stopp {0}");
            texts.Add(LangKey.AlarmClock.ToString().ToLower(), "Alarm-clock");
            texts.Add(LangKey.CuckooClock.ToString().ToLower(), "Cuckoo Clock");
            texts.Add(LangKey.KillProcess.ToString().ToLower(), "Kill Process");
            texts.Add(LangKey.Programs.ToString().ToLower(), "Programs");
            texts.Add(LangKey.Malware.ToString().ToLower(), "Malware");
            texts.Add(LangKey.ScanPC.ToString().ToLower(), "Scan PC");
            texts.Add(LangKey.CleanPC.ToString().ToLower(), "Clean PC");
            texts.Add(LangKey.ScanReport.ToString().ToLower(), "Scan Report");
            texts.Add(LangKey.ScanResult.ToString().ToLower(), "Virulent objects found: {0}");
            texts.Add(LangKey.ObjectsFound.ToString().ToLower(), "{0} object(s) found");
            texts.Add(LangKey.ObjectsRemoved.ToString().ToLower(), "{0} object(s) removed");

            return textDB;
        }
    }
}

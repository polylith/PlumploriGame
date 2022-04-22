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
            texts.Add(LangKey.ChargeState.ToString().ToLower(), "Ladezustand");
            texts.Add(LangKey.CurrentState.ToString().ToLower(), "Aktueller Zustand");
            texts.Add(LangKey.Charging.ToString().ToLower(), "{0} lädt auf");
            texts.Add(LangKey.Resting.ToString().ToLower(), "{0} befindet sich im Ruhezustand");
            texts.Add(LangKey.Starting.ToString().ToLower(), "{0} startet");
            texts.Add(LangKey.Scanning.ToString().ToLower(), "{0} scannt gerade");
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
            texts.Add(LangKey.IsConnected.ToString().ToLower(), "{0} ist verbunden");
            texts.Add(LangKey.IsDisconnected.ToString().ToLower(), "{0} ist nicht verbunden");
            texts.Add(LangKey.Install.ToString().ToLower(), "{0} installieren");
            texts.Add(LangKey.Installed.ToString().ToLower(), "{0} installiert");
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
            texts.Add(LangKey.SwitchOn.ToString().ToLower(), "{0} einschalten");
            texts.Add(LangKey.SwitchOff.ToString().ToLower(), "{0} ausschalten");
            
            texts.Add(LangKey.SwitchPlayer.ToString().ToLower(), "Spieler wechseln");
            texts.Add(LangKey.Player.ToString().ToLower(), "Spieler {0}");

            texts.Add(LangKey.Shutdown.ToString().ToLower(), "Ausschalten");
            texts.Add(LangKey.Reboot.ToString().ToLower(), "Neu starten");
            texts.Add(LangKey.LogOff.ToString().ToLower(), "Abmelden");
            
            texts.Add(LangKey.EG.ToString().ToLower(), "EG");
            texts.Add(LangKey.UG.ToString().ToLower(), "UG");
            texts.Add(LangKey.OG.ToString().ToLower(), "OG");

            texts.Add(LangKey.TV.ToString().ToLower(), "Fernseher");
            texts.Add(LangKey.Router.ToString().ToLower(), "Router");
            texts.Add(LangKey.PC.ToString().ToLower(), "Rechner");
            texts.Add(LangKey.PCAppShopApp.ToString().ToLower(), "App Shop");
            texts.Add(LangKey.AntivirusApp.ToString().ToLower(), "Antivirus");
            texts.Add(LangKey.TaskMgrApp.ToString().ToLower(), "Prozesse");
            texts.Add(LangKey.Progress.ToString().ToLower(), "Fortschritt");
            texts.Add(LangKey.Programs.ToString().ToLower(), "Programme");
            texts.Add(LangKey.Malware.ToString().ToLower(), "Schadsoftware");
            texts.Add(LangKey.CalcApp.ToString().ToLower(), "Taschenrechner");
            texts.Add(LangKey.PromptApp.ToString().ToLower(), "Eingabeaufforderung");
            texts.Add(LangKey.ProfiBrainApp.ToString().ToLower(), "Profi Brain");
            texts.Add(LangKey.FourInARowApp.ToString().ToLower(), "Verbinde Vier");
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
            texts.Add(LangKey.Info.ToString().ToLower(), "Info {0}");
            texts.Add(LangKey.Warning.ToString().ToLower(), "Hinweis {0}");
            texts.Add(LangKey.Question.ToString().ToLower(), "Frage {0}");
            texts.Add(LangKey.Prompt.ToString().ToLower(), "Abfrage");
            texts.Add(LangKey.Lid.ToString().ToLower(), "Deckel");
            texts.Add(LangKey.Environment.ToString().ToLower(), "Umgebung");
            texts.Add(LangKey.NetworkConnection.ToString().ToLower(), "Netzwerkverbindung");
            texts.Add(LangKey.System.ToString().ToLower(), "System");
            texts.Add(LangKey.App.ToString().ToLower(), "Anwendung");
            texts.Add(LangKey.Game.ToString().ToLower(), "Spiel");
            texts.Add(LangKey.Clock.ToString().ToLower(), "Uhr");
            texts.Add(LangKey.CurrentTime.ToString().ToLower(), "Aktuelle Zeit");
            texts.Add(LangKey.Alarms.ToString().ToLower(), "Alarmzeiten");
            texts.Add(LangKey.Alarm.ToString().ToLower(), "Alarm");
            texts.Add(LangKey.Stopp.ToString().ToLower(), "Stoppe {0}");
            texts.Add(LangKey.AlarmClock.ToString().ToLower(), "Wecker");
            texts.Add(LangKey.CuckooClock.ToString().ToLower(), "Kuckucksuhr");
            texts.Add(LangKey.KillProcess.ToString().ToLower(), "Prozess beenden");
            texts.Add(LangKey.Scan.ToString().ToLower(), "{0} scannen");
            texts.Add(LangKey.Clean.ToString().ToLower(), "{0} reinigen");
            texts.Add(LangKey.ScanReport.ToString().ToLower(), "Scan Report");
            texts.Add(LangKey.ScanResult.ToString().ToLower(), "Infizierte Objekte gefunden: {0}");
            texts.Add(LangKey.ObjectsFound.ToString().ToLower(), "{0} Objekt(e) gefunden");
            texts.Add(LangKey.ObjectsRemoved.ToString().ToLower(), "{0} Objekt(e) entfernt");
            texts.Add(LangKey.Data.ToString().ToLower(), "Daten");
            texts.Add(LangKey.Yes.ToString().ToLower(), "Ja");
            texts.Add(LangKey.No.ToString().ToLower(), "Nein");
            texts.Add(LangKey.Ok.ToString().ToLower(), "Ok");
            texts.Add(LangKey.Cancel.ToString().ToLower(), "Abbrechen");
            texts.Add(LangKey.Option.ToString().ToLower(), "Option");
            texts.Add(LangKey.Options.ToString().ToLower(), "Optionen");
            texts.Add(LangKey.AppLaunchError.ToString().ToLower(), "<b><i>{0}</i> konnte nicht gestartet werden.</b><br>Das System kann diese Anwendung zur Zeit nicht starten. Bitte versuchen Sie es noch einmal.<br><b>Wenn das Problem weiterhin besteht, wenden Sie sich an Ihren Administrator.</b>");
            texts.Add(LangKey.AutoSync.ToString().ToLower(), "Automatisch synchronisiert");

            texts.Add(LangKey.CodeLength.ToString().ToLower(), "Länge des Codes");
            texts.Add(LangKey.NumberOfColors.ToString().ToLower(), "Anzahl der Farben");
            texts.Add(LangKey.NumberOfPlayers.ToString().ToLower(), "Spieleranzahl");
            texts.Add(LangKey.Mode.ToString().ToLower(), "Modus");
            texts.Add(LangKey.AllowEmptyInputs.ToString().ToLower(), "Leere Eingaben zulassen");
            texts.Add(LangKey.ShowOnlyUsedColors.ToString().ToLower(), "Nur verwendete Farben zeigen");
            texts.Add(LangKey.OrderedEvaluation.ToString().ToLower(), "Geordnete Auswertung");
            texts.Add(LangKey.Level.ToString().ToLower(), "Level");
            texts.Add(LangKey.UserMode.ToString().ToLower(), "Spielermodus");
            texts.Add(LangKey.SimpleMode.ToString().ToLower(), "Leichter Modus");
            texts.Add(LangKey.ProfiMode.ToString().ToLower(), "Profimodus");
            texts.Add(LangKey.MasterMode.ToString().ToLower(), "Meistermodus");

            texts = new Dictionary<string, string>();
            textDB.Add(SystemLanguage.English, texts);
            langInfo.Add(SystemLanguage.English, new CultureInfo("en-US"));

            texts.Add(SystemLanguage.English.ToString().ToLower(), "English");
            texts.Add(SystemLanguage.German.ToString().ToLower(), "German");

            texts.Add(LangKey.Off.ToString().ToLower(), "{0} is off");
            texts.Add(LangKey.On.ToString().ToLower(), "{0} is on");
            texts.Add(LangKey.ChargeState.ToString().ToLower(), "Charge State");
            texts.Add(LangKey.CurrentState.ToString().ToLower(), "Current State");
            texts.Add(LangKey.Charging.ToString().ToLower(), "{0} is charging");
            texts.Add(LangKey.Resting.ToString().ToLower(), "{0} is resting");
            texts.Add(LangKey.Starting.ToString().ToLower(), "{0} is starting");
            texts.Add(LangKey.Scanning.ToString().ToLower(), "{0} is scanning");
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
            texts.Add(LangKey.IsConnected.ToString().ToLower(), "{0} is connected");
            texts.Add(LangKey.IsDisconnected.ToString().ToLower(), "{0} is disconnected");
            texts.Add(LangKey.Install.ToString().ToLower(), "{0} install");
            texts.Add(LangKey.Installed.ToString().ToLower(), "{0} installed");
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
            texts.Add(LangKey.SwitchOn.ToString().ToLower(), "Switch on {0}");
            texts.Add(LangKey.SwitchOff.ToString().ToLower(), "Switch off {0}");

            texts.Add(LangKey.SwitchPlayer.ToString().ToLower(), "Switch Player");
            texts.Add(LangKey.Player.ToString().ToLower(), "Player {0}");

            texts.Add(LangKey.Shutdown.ToString().ToLower(), "Shutdown");
            texts.Add(LangKey.Reboot.ToString().ToLower(), "Reboot");
            texts.Add(LangKey.LogOff.ToString().ToLower(), "Log off");
            
            texts.Add(LangKey.EG.ToString().ToLower(), "G");
            texts.Add(LangKey.UG.ToString().ToLower(), "B");
            texts.Add(LangKey.OG.ToString().ToLower(), "");

            texts.Add(LangKey.TV.ToString().ToLower(), "TV");
            texts.Add(LangKey.Router.ToString().ToLower(), "Router");
            texts.Add(LangKey.PC.ToString().ToLower(), "Computer");
            texts.Add(LangKey.PCAppShopApp.ToString().ToLower(), "App Shop");
            texts.Add(LangKey.AntivirusApp.ToString().ToLower(), "Antivirus");
            texts.Add(LangKey.TaskMgrApp.ToString().ToLower(), "Task Manager");
            texts.Add(LangKey.CalcApp.ToString().ToLower(), "Calculator");
            texts.Add(LangKey.PromptApp.ToString().ToLower(), "Prompt");
            texts.Add(LangKey.ProfiBrainApp.ToString().ToLower(), "Profi Brain");
            texts.Add(LangKey.FourInARowApp.ToString().ToLower(), "Four in a Row");
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
            texts.Add(LangKey.Info.ToString().ToLower(), "Info {0}");
            texts.Add(LangKey.Warning.ToString().ToLower(), "Warning {0}");
            texts.Add(LangKey.Question.ToString().ToLower(), "Question {0}");
            texts.Add(LangKey.Prompt.ToString().ToLower(), "Prompt");
            texts.Add(LangKey.Image.ToString().ToLower(), "Image");
            texts.Add(LangKey.Lid.ToString().ToLower(), "Lid");
            texts.Add(LangKey.Clock.ToString().ToLower(), "Clock");
            texts.Add(LangKey.Environment.ToString().ToLower(), "Environment");
            texts.Add(LangKey.NetworkConnection.ToString().ToLower(), "Network Connection");
            texts.Add(LangKey.System.ToString().ToLower(), "System");
            texts.Add(LangKey.App.ToString().ToLower(), "App");
            texts.Add(LangKey.Game.ToString().ToLower(), "Game");
            texts.Add(LangKey.CurrentTime.ToString().ToLower(), "current time");
            texts.Add(LangKey.Alarms.ToString().ToLower(), "Alarm Times");
            texts.Add(LangKey.Alarm.ToString().ToLower(), "Alarm");
            texts.Add(LangKey.Stopp.ToString().ToLower(), "Stopp {0}");
            texts.Add(LangKey.AlarmClock.ToString().ToLower(), "Alarm-clock");
            texts.Add(LangKey.CuckooClock.ToString().ToLower(), "Cuckoo Clock");
            texts.Add(LangKey.KillProcess.ToString().ToLower(), "Kill Process");
            texts.Add(LangKey.Progress.ToString().ToLower(), "Progress");
            texts.Add(LangKey.Programs.ToString().ToLower(), "Programs");
            texts.Add(LangKey.Malware.ToString().ToLower(), "Malware");
            texts.Add(LangKey.Scan.ToString().ToLower(), "Scan {0}");
            texts.Add(LangKey.Clean.ToString().ToLower(), "Clean {0}");
            texts.Add(LangKey.ScanReport.ToString().ToLower(), "Scan Report");
            texts.Add(LangKey.ScanResult.ToString().ToLower(), "Virulent objects found: {0}");
            texts.Add(LangKey.ObjectsFound.ToString().ToLower(), "{0} object(s) found");
            texts.Add(LangKey.ObjectsRemoved.ToString().ToLower(), "{0} object(s) removed");
            texts.Add(LangKey.Data.ToString().ToLower(), "Data");
            texts.Add(LangKey.Yes.ToString().ToLower(), "Yes");
            texts.Add(LangKey.No.ToString().ToLower(), "No");
            texts.Add(LangKey.Ok.ToString().ToLower(), "Ok");
            texts.Add(LangKey.Cancel.ToString().ToLower(), "Cancel");
            texts.Add(LangKey.Option.ToString().ToLower(), "Option");
            texts.Add(LangKey.Options.ToString().ToLower(), "Options");
            texts.Add(LangKey.AppLaunchError.ToString().ToLower(), "<b><i>{0}</i> failed to start.</b><br> System is unable to start this application at this time. Please retry again.<br><b>If the problem persits, contact your administrator.</b>");
            texts.Add(LangKey.AutoSync.ToString().ToLower(), "Automatically synchronized");

            texts.Add(LangKey.CodeLength.ToString().ToLower(), "Code Length");
            texts.Add(LangKey.NumberOfColors.ToString().ToLower(), "Number of colors");
            texts.Add(LangKey.NumberOfPlayers.ToString().ToLower(), "Number of players");
            texts.Add(LangKey.Mode.ToString().ToLower(), "Modus");
            texts.Add(LangKey.AllowEmptyInputs.ToString().ToLower(), "Allow empty inputs");
            texts.Add(LangKey.ShowOnlyUsedColors.ToString().ToLower(), "Only show used colors");
            texts.Add(LangKey.OrderedEvaluation.ToString().ToLower(), "Ordered evaluation");
            texts.Add(LangKey.Level.ToString().ToLower(), "Level");
            texts.Add(LangKey.UserMode.ToString().ToLower(), "User Mode");
            texts.Add(LangKey.SimpleMode.ToString().ToLower(), "Simple Mode");
            texts.Add(LangKey.ProfiMode.ToString().ToLower(), "Profi Mode");
            texts.Add(LangKey.MasterMode.ToString().ToLower(), "Master Mode");

            return textDB;
        }
    }
}

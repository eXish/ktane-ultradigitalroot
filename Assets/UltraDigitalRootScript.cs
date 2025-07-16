using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;
using System;

public class UltraDigitalRootScript : MonoBehaviour {

    public KMAudio audio;
    public KMBombInfo bomb;
    public KMSelectable[] buttons;
    public Renderer[] btnRends;
    public TextMesh[] btnTexts;
    public TextMesh[] displays;
    public Color[] displayTextColors;
    public Color[] btnTextColors;
    public Material[] btnColors;

    private bool held;
    private bool activated;
    private bool validTime;
    private int[] btnColorIndexes = new int[4];
    private int[] btnTextColorIndexes = new int[4];
    private int beingHeld;
    private int step1Sum;
    private int step1Root;
    private int step3Sum;
    private int correctBtn;
    private int correctHold;
    private int correctRelease;
    private string[] posNames = new string[] { "1st", "2nd", "3rd", "4th" };
    private string[] displayChars = new string[15];
    private string step1Binary;
    private string step2Binary;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        moduleSolved = false;
        foreach (KMSelectable obj in buttons)
        {
            KMSelectable pressed = obj;
            pressed.OnInteract += delegate () { HoldButton(pressed); return false; };
            pressed.OnInteractEnded += delegate () { ReleaseButton(pressed); };
        }
        GetComponent<KMBombModule>().OnActivate += OnActivate;
    }

    void Start () {
        if (!activated)
        {
            for (int i = 0; i < displays.Length; i++)
                displays[i].text = "";
        }
        GenerateModule();
    }

    void OnActivate()
    {
        for (int i = 0; i < displays.Length; i++)
            displays[i].text = displayChars[i];
        activated = true;
    }

    void HoldButton(KMSelectable pressed)
    {
        if (moduleSolved != true && held != true && activated != false)
        {
            pressed.AddInteractionPunch(0.5f);
            audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
            pressed.transform.localPosition = new Vector3(pressed.transform.localPosition.x, 0.012f, pressed.transform.localPosition.z);
            held = true;
            validTime = true;
            beingHeld = Array.IndexOf(buttons, pressed);
            Debug.LogFormat("[Ultra Digital Root #{0}] ==Input==", moduleId);
            Debug.LogFormat("[Ultra Digital Root #{0}] Held the {1} button when the total number of seconds remaining was {2}", moduleId, posNames[beingHeld], (int)bomb.GetTime());
            if (!((int)bomb.GetTime()).ToString().Contains(correctHold.ToString()))
                validTime = false;
        }
    }

    void ReleaseButton(KMSelectable pressed)
    {
        if (moduleSolved != true && held != false && activated != false)
        {
            audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, pressed.transform);
            pressed.transform.localPosition = new Vector3(pressed.transform.localPosition.x, 0.015f, pressed.transform.localPosition.z);
            held = false;
            Debug.LogFormat("[Ultra Digital Root #{0}] Released the {1} button when the total number of seconds remaining was {2}", moduleId, posNames[beingHeld], (int)bomb.GetTime());
            if (!((int)bomb.GetTime()).ToString().Contains(correctRelease.ToString()))
                validTime = false;
            if (beingHeld != correctBtn)
            {
                Debug.LogFormat("[Ultra Digital Root #{0}] Incorrect button used! Strike! Resetting module...", moduleId);
                GetComponent<KMBombModule>().HandleStrike();
                Start();
                return;
            }
            if (!validTime)
            {
                Debug.LogFormat("[Ultra Digital Root #{0}] Button held or released on incorrect time! Strike! Resetting module...", moduleId);
                GetComponent<KMBombModule>().HandleStrike();
                Start();
                return;
            }
            Debug.LogFormat("[Ultra Digital Root #{0}] The correct button was held and released on the correct times, module disarmed!", moduleId);
            GetComponent<KMBombModule>().HandlePass();
            moduleSolved = true;
        }
    }

    private void GenerateModule()
    {
        string[] buttonTextChoices = new string[] { "yes", "yee", "ya", "yea", "y", "no", "na", "nah", "nay", "n" };
        string[] choices = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        redo:
        step1Sum = 0;
        step1Binary = "";
        step2Binary = "";
        for (int i = 0; i < displayChars.Length; i++)
        {
            int chance = UnityEngine.Random.Range(1, 101);
            if (chance < 21)
            {
                displays[i].color = displayTextColors[1];
                string let = choices[UnityEngine.Random.Range(0, choices.Length)];
                displayChars[i] = let;
                if (!bomb.GetSerialNumberLetters().Contains(let.ToCharArray()[0]))
                    step1Sum += Array.IndexOf(choices, let) + 1;
            }
            else
            {
                displays[i].color = displayTextColors[0];
                int num = UnityEngine.Random.Range(0, 100);
                displayChars[i] = num.ToString();
                step1Sum += num;
            }
        }
        if (activated)
        {
            for (int i = 0; i < displays.Length; i++)
                displays[i].text = displayChars[i];
        }
        for (int i = 0; i < 4; i++)
        {
            int ind1 = UnityEngine.Random.Range(0, btnColors.Length);
            btnRends[i].material = btnColors[ind1];
            btnColorIndexes[i] = ind1;
            int ind2 = UnityEngine.Random.Range(0, btnTextColors.Length);
            btnTexts[i].color = btnTextColors[ind2];
            btnTextColorIndexes[i] = ind2;
            btnTexts[i].text = buttonTextChoices[UnityEngine.Random.Range(0, buttonTextChoices.Length)];
        }
        step1Root = MultiRoot(step1Sum, false);
        bool waszero = false;
        if (step1Root == 0)
        {
            waszero = true;
            step1Root = AddRoot(step1Sum, false);
            if (step1Root == 0)
                goto redo;
        }
        switch (step1Root)
        {
            case 1:
                if (bomb.IsIndicatorPresent("SND")) step1Binary += "1"; else step1Binary += "0";
                if (bomb.IsPortPresent(Port.Serial)) step1Binary += "1"; else step1Binary += "0";
                if (bomb.IsTwoFactorPresent()) step1Binary += "1"; else step1Binary += "0";
                if (bomb.IsPortPresent(Port.PS2)) step1Binary += "1"; else step1Binary += "0";
                break;
            case 2:
                if (ContainsAny(bomb.GetSerialNumber(), new char[] { '0', '2', '4', '6', '8' })) step1Binary += "1"; else step1Binary += "0";
                if (bomb.IsIndicatorPresent("CAR")) step1Binary += "1"; else step1Binary += "0";
                if (bomb.GetBatteryCount() % 2 == 0) step1Binary += "1"; else step1Binary += "0";
                if (bomb.IsPortPresent(Port.CompositeVideo)) step1Binary += "1"; else step1Binary += "0";
                break;
            case 3:
                if (bomb.IsPortPresent(Port.USB)) step1Binary += "1"; else step1Binary += "0";
                if (bomb.GetBatteryHolderCount() % 2 == 1) step1Binary += "1"; else step1Binary += "0";
                if (bomb.IsIndicatorPresent("TRN")) step1Binary += "1"; else step1Binary += "0";
                if (bomb.GetBatteryCount() % 2 == 1) step1Binary += "1"; else step1Binary += "0";
                break;
            case 4:
                if (ContainsAny(bomb.GetSerialNumber(), new char[] { 'A', 'E', 'I', 'O', 'U' })) step1Binary += "1"; else step1Binary += "0";
                if (IsTimeOfDayPresent()) step1Binary += "1"; else step1Binary += "0";
                if (bomb.IsIndicatorPresent("MSA")) step1Binary += "1"; else step1Binary += "0";
                if (bomb.GetBatteryCount(Battery.AA) > 0) step1Binary += "1"; else step1Binary += "0";
                break;
            case 5:
                if (bomb.IsPortPresent(Port.ComponentVideo)) step1Binary += "1"; else step1Binary += "0";
                if (bomb.IsIndicatorPresent("NSA")) step1Binary += "1"; else step1Binary += "0";
                if (bomb.IsIndicatorPresent("FRK")) step1Binary += "1"; else step1Binary += "0";
                if (IsNumberedIndicatorPresent()) step1Binary += "1"; else step1Binary += "0";
                break;
            case 6:
                if (ContainsAny(bomb.GetSerialNumber(), new char[] { 'B', 'C', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'V', 'W', 'X', 'Y', 'Z' })) step1Binary += "1"; else step1Binary += "0";
                if (bomb.IsIndicatorPresent("SIG")) step1Binary += "1"; else step1Binary += "0";
                if (bomb.IsPortPresent(Port.StereoRCA)) step1Binary += "1"; else step1Binary += "0";
                if (bomb.IsPortPresent(Port.VGA)) step1Binary += "1"; else step1Binary += "0";
                break;
            case 7:
                if (bomb.IsPortPresent(Port.HDMI)) step1Binary += "1"; else step1Binary += "0";
                if (bomb.GetBatteryHolderCount() % 2 == 0) step1Binary += "1"; else step1Binary += "0";
                if (bomb.IsIndicatorPresent("IND")) step1Binary += "1"; else step1Binary += "0";
                if (ContainsAny(bomb.GetSerialNumber(), new char[] { '1', '3', '5', '7', '9' })) step1Binary += "1"; else step1Binary += "0";
                break;
            case 8:
                if (bomb.IsPortPresent(Port.PCMCIA)) step1Binary += "1"; else step1Binary += "0";
                if (bomb.IsIndicatorPresent("CLR")) step1Binary += "1"; else step1Binary += "0";
                if (bomb.IsPortPresent(Port.DVI)) step1Binary += "1"; else step1Binary += "0";
                if (IsDateOfManufacturePresent()) step1Binary += "1"; else step1Binary += "0";
                break;
            case 9:
                if (bomb.IsIndicatorPresent("FRQ")) step1Binary += "1"; else step1Binary += "0";
                if (bomb.GetBatteryCount(Battery.D) > 0) step1Binary += "1"; else step1Binary += "0";
                if (IsEncryptedIndicatorPresent()) step1Binary += "1"; else step1Binary += "0";
                if (bomb.IsPortPresent(Port.Parallel)) step1Binary += "1"; else step1Binary += "0";
                break;
        }
        int[] binaryCts = new int[4];
        for (int i = 0; i < 4; i++)
        {
            if (btnTexts[i].text.EqualsAny("yea", "ya", "nah"))
            {
                if (btnColorIndexes[i] > 1)
                {
                    if (RedBtnCt() <= 2)
                        binaryCts[0]++;
                }
                else
                {
                    if (btnTextColorIndexes[i] < 2)
                        binaryCts[1]++;
                }
            }
            else
            {
                if (btnColorIndexes[i] == btnTextColorIndexes[i])
                {
                    if (DispLetCt(choices) >= 4)
                        binaryCts[2]++;
                }
                else
                {
                    if (btnTexts[i].text.StartsWith("y"))
                        binaryCts[3]++;
                }
            }
        }
        for (int i = 0; i < 4; i++)
        {
            if (binaryCts[i] % 2 == 0)
                step2Binary += "1";
            else
                step2Binary += "0";
        }
        if (!ValidBinary(step1Binary, step2Binary))
            goto redo;
        else
            correctBtn = CorrectButton(step1Binary, step2Binary);
        step3Sum = step1Sum + Convert.ToInt32(step1Binary, 2) + Convert.ToInt32(step2Binary, 2);
        correctHold = AddRoot(step3Sum, false) + AddRoot(step3Sum, true);
        correctRelease = MultiRoot(step3Sum, false) + MultiRoot(step3Sum, true);
        Debug.LogFormat("[Ultra Digital Root #{0}] ==Displays And Buttons==", moduleId);
        Debug.LogFormat("[Ultra Digital Root #{0}] The displays show: {1}", moduleId, displayChars.Join(" "));
        string[] colNames = new string[] { "red", "green", "blue", "yellow" };
        Debug.LogFormat("[Ultra Digital Root #{0}] The button's colors are: {1} {2} {3} {4}", moduleId, colNames[btnColorIndexes[0]], colNames[btnColorIndexes[1]], colNames[btnColorIndexes[2]], colNames[btnColorIndexes[3]]);
        Debug.LogFormat("[Ultra Digital Root #{0}] The button's text colors are: {1} {2} {3} {4}", moduleId, colNames[btnTextColorIndexes[0]], colNames[btnTextColorIndexes[1]], colNames[btnTextColorIndexes[2]], colNames[btnTextColorIndexes[3]]);
        Debug.LogFormat("[Ultra Digital Root #{0}] The button's texts are: {1} {2} {3} {4}", moduleId, btnTexts[0].text, btnTexts[1].text, btnTexts[2].text, btnTexts[3].text);
        Debug.LogFormat("[Ultra Digital Root #{0}] ==Step 1==", moduleId);
        Debug.LogFormat("[Ultra Digital Root #{0}] Sum of each display's numbers and letters NOT in serial: {1}", moduleId, step1Sum);
        Debug.LogFormat("[Ultra Digital Root #{0}] Sum's multiplicative digital root {1}", moduleId, waszero ? "is 0, using additive digital root which is " + step1Root : "isn't 0, using multiplicative digital root which is " + step1Root);
        List<string> present = new List<string>();
        for (int i = 0; i < 4; i++)
        {
            if (step1Binary[i] == '1')
                present.Add((i + 1).ToString());
        }
        Debug.LogFormat("[Ultra Digital Root #{0}] Pieces of edgework in column {1} that are present from top to bottom: {2}", moduleId, step1Root, present.Count == 0 ? "None" : present.Join(" "));
        Debug.LogFormat("[Ultra Digital Root #{0}] This results in the 4-digit binary number: {1}", moduleId, step1Binary);
        Debug.LogFormat("[Ultra Digital Root #{0}] ==Step 2==", moduleId);
        Debug.LogFormat("[Ultra Digital Root #{0}] Each position at the bottom row of the flowchart has this number of times it is reached and true: {1}", moduleId, binaryCts.Join(" "));
        Debug.LogFormat("[Ultra Digital Root #{0}] This results in the 4-digit binary number: {1}", moduleId, step2Binary);
        Debug.LogFormat("[Ultra Digital Root #{0}] The position that has a different digit in the binary numbers is: {1}", moduleId, correctBtn + 1);
        Debug.LogFormat("[Ultra Digital Root #{0}] ==Step 3==", moduleId);
        Debug.LogFormat("[Ultra Digital Root #{0}] Sum of step 1's sum and step 1 and 2's binary numbers converted to decimal ({1} and {2} respectively) is: {3}", moduleId, Convert.ToInt32(step1Binary, 2), Convert.ToInt32(step2Binary, 2), step3Sum);
        Debug.LogFormat("[Ultra Digital Root #{0}] Sum's additive digital root ({1}) + sum's additive persistence ({2}) is: {3}", moduleId, AddRoot(step3Sum, false), AddRoot(step3Sum, true), correctHold);
        Debug.LogFormat("[Ultra Digital Root #{0}] Sum's multiplicative digital root ({1}) + sum's multiplicative persistence ({2}) is: {3}", moduleId, MultiRoot(step3Sum, false), MultiRoot(step3Sum, true), correctRelease);
        Debug.LogFormat("[Ultra Digital Root #{0}] ==Answer==", moduleId);
        Debug.LogFormat("[Ultra Digital Root #{0}] Hold the {1} button in reading order when the total number of seconds remaining contains {2} and release it when it contains {3}", moduleId, posNames[correctBtn], correctHold, correctRelease);
    }

    private bool ContainsAny(string item, char[] items)
    {
        foreach (char i in item)
        {
            if (items.Contains(i))
                return true;
        }

        return false;
    }

    private bool IsTimeOfDayPresent()
    {
        if (bomb.QueryWidgets("day", "").Count != 0)
            return true;
        else
            return false;
    }

    private bool IsNumberedIndicatorPresent()
    {
        for (int i = 0; i < bomb.QueryWidgets(KMBombInfo.QUERYKEY_GET_INDICATOR, "").Count; i++)
        {
            if (bomb.QueryWidgets(KMBombInfo.QUERYKEY_GET_INDICATOR, "")[i].Contains("\"label\"") && bomb.QueryWidgets(KMBombInfo.QUERYKEY_GET_INDICATOR, "")[i].Contains("\"on\"") && bomb.QueryWidgets(KMBombInfo.QUERYKEY_GET_INDICATOR, "")[i].Contains("\"display\"") && bomb.QueryWidgets(KMBombInfo.QUERYKEY_GET_INDICATOR, "")[i].Contains("\"isNumbered\""))
                return true;
        }
        return false;
    }

    private bool IsDateOfManufacturePresent()
    {
        if (bomb.QueryWidgets("manufacture", "").Count != 0)
            return true;
        else
            return false;
    }

    private bool IsEncryptedIndicatorPresent()
    {
        for (int i = 0; i < bomb.QueryWidgets(KMBombInfo.QUERYKEY_GET_INDICATOR, "").Count; i++)
        {
            if (bomb.QueryWidgets(KMBombInfo.QUERYKEY_GET_INDICATOR, "")[i].Contains("\"label\"") && bomb.QueryWidgets(KMBombInfo.QUERYKEY_GET_INDICATOR, "")[i].Contains("\"on\"") && bomb.QueryWidgets(KMBombInfo.QUERYKEY_GET_INDICATOR, "")[i].Contains("\"display\"") && !bomb.QueryWidgets(KMBombInfo.QUERYKEY_GET_INDICATOR, "")[i].Contains("\"isNumbered\""))
                return true;
        }
        return false;
    }

    private int RedBtnCt()
    {
        int ct = 0;
        for (int i = 0; i < 4; i++)
        {
            if (btnRends[i].material.name.Replace(" (Instance)", "") == "red")
                ct++;
        }
        return ct;
    }

    private int DispLetCt(string[] letters)
    {
        int ct = 0;
        for (int i = 0; i < 15; i++)
        {
            if (letters.Contains(displayChars[i]))
                ct++;
        }
        return ct;
    }

    private bool ValidBinary(string bin1, string bin2)
    {
        int dif = 0;
        for (int i = 0; i < 4; i++)
        {
            if (bin1[i] != bin2[i])
                dif++;
        }
        if (dif == 1)
            return true;
        else
            return false;
    }

    private int CorrectButton(string bin1, string bin2)
    {
        for (int i = 0; i < 4; i++)
        {
            if (bin1[i] != bin2[i])
                return i;
        }
        return -1;
    }

    private int AddRoot(int num, bool persist)
    {
        int persNum = 0;
        string combo = "" + num;
        while (combo.Length > 1)
        {
            int total = 0;
            for (int i = 0; i < combo.Length; i++)
            {
                int temp = 0;
                int.TryParse(combo.Substring(i, 1), out temp);
                total += temp;
            }
            combo = total + "";
            persNum++;
        }
        if (persist)
            return persNum;
        else
        {
            int temp2 = 0;
            int.TryParse(combo, out temp2);
            return temp2;
        }
    }

    private int MultiRoot(int num, bool persist)
    {
        int persNum = 0;
        string combo = "" + num;
        while (combo.Length > 1)
        {
            int total = 1;
            for (int i = 0; i < combo.Length; i++)
            {
                int temp = 0;
                int.TryParse(combo.Substring(i, 1), out temp);
                total *= temp;
            }
            combo = total + "";
            persNum++;
        }
        if (persist)
            return persNum;
        else
        {
            int temp2 = 0;
            int.TryParse(combo, out temp2);
            return temp2;
        }
    }

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} hold <btn> at/on <##> [Holds the specified button when the number of seconds remaining is '##'] | !{0} release at/on <##> [Releases the held button when the number of seconds remaining is '##'] | Valid buttons are 1-4 in reading order";
    bool ZenModeActive;
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*hold\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length > 4)
            {
                yield return "sendtochaterror Too many parameters!";
            }
            else if (parameters.Length == 4)
            {
                if (parameters[1].EqualsAny("1", "2", "3", "4"))
                {
                    if (parameters[2].ToLower().EqualsAny("at", "on"))
                    {
                        int temp = 0;
                        if (int.TryParse(parameters[3], out temp))
                        {
                            if (temp > -1)
                            {
                                if (held)
                                {
                                    yield return "sendtochaterror A button is already being held!";
                                    yield break;
                                }
                                var music = false;
                                if (ZenModeActive)
                                {
                                    if (temp - (int)bomb.GetTime() > 15) music = true;
                                }
                                else
                                {
                                    if ((int)bomb.GetTime() - temp > 15) music = true;
                                }
                                if (music) yield return "waiting music";
                                while ((int)bomb.GetTime() != temp) { yield return "trycancel"; }
                                if (music) yield return "end waiting music";
                                buttons[int.Parse(parameters[1]) - 1].OnInteract();
                            }
                            else
                            {
                                yield return "sendtochaterror The specified number of seconds '" + parameters[3] + "' is less than 0!";
                            }
                        }
                        else
                        {
                            yield return "sendtochaterror!f The specified number of seconds '" + parameters[3] + "' is invalid!";
                        }
                    }
                    else
                    {
                        yield return "sendtochaterror!f The specified parameter '" + parameters[2] + "' is invalid! Expected 'at' or 'on'!";
                    }
                }
                else
                {
                    yield return "sendtochaterror!f The specified button '" + parameters[1] + "' is invalid!";
                }
            }
            else if (parameters.Length == 3)
            {
                if (parameters[1].EqualsAny("1", "2", "3", "4"))
                {
                    if (parameters[2].ToLower().EqualsAny("at", "on"))
                    {
                        yield return "sendtochaterror Please specify a number of seconds to hold the button at/on!";
                    }
                    else
                    {
                        yield return "sendtochaterror!f The specified parameter '" + parameters[2] + "' is invalid! Expected 'at' or 'on'!";
                    }
                }
                else
                {
                    yield return "sendtochaterror!f The specified button '" + parameters[1] + "' is invalid!";
                }
            }
            else if (parameters.Length == 2)
            {
                if (parameters[1].EqualsAny("1", "2", "3", "4"))
                {
                    yield return "sendtochaterror Please specify the word 'at' or 'on' and a number of seconds to hold the button at/on!";
                }
                else
                {
                    yield return "sendtochaterror!f The specified button '" + parameters[1] + "' is invalid!";
                }
            }
            else if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify a button, the word 'at' or 'on', and a number of seconds to hold the button at/on!";
            }
            yield break;
        }
        if (Regex.IsMatch(parameters[0], @"^\s*release\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length > 3)
            {
                yield return "sendtochaterror Too many parameters!";
            }
            else if (parameters.Length == 3)
            {
                if (parameters[1].ToLower().EqualsAny("at", "on"))
                {
                    int temp = 0;
                    if (int.TryParse(parameters[2], out temp))
                    {
                        if (temp > -1)
                        {
                            if (!held)
                            {
                                yield return "sendtochaterror A button is not currently being held!";
                                yield break;
                            }
                            var music = false;
                            if (ZenModeActive)
                            {
                                if (temp - (int)bomb.GetTime() > 15) music = true;
                            }
                            else
                            {
                                if ((int)bomb.GetTime() - temp > 15) music = true;
                            }
                            if (music) yield return "waiting music";
                            while ((int)bomb.GetTime() != temp) { yield return "trycancel"; }
                            if (music) yield return "end waiting music";
                            buttons[beingHeld].OnInteractEnded();
                        }
                        else
                        {
                            yield return "sendtochaterror The specified number of seconds '" + parameters[2] + "' is less than 0!";
                        }
                    }
                    else
                    {
                        yield return "sendtochaterror!f The specified number of seconds '" + parameters[2] + "' is invalid!";
                    }
                }
                else
                {
                    yield return "sendtochaterror!f The specified parameter '" + parameters[1] + "' is invalid! Expected 'at' or 'on'!";
                }
            }
            else if (parameters.Length == 2)
            {
                if (parameters[1].ToLower().EqualsAny("at", "on"))
                {
                    yield return "sendtochaterror Please specify a number of seconds to release the button at/on!";
                }
                else
                {
                    yield return "sendtochaterror!f The specified parameter '" + parameters[1] + "' is invalid! Expected 'at' or 'on'!";
                }
            }
            else if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify the word 'at' or 'on' and a number of seconds to release the button at/on!";
            }
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        while (!activated) { yield return true; }
        if (held && (correctBtn != beingHeld || !validTime))
        {
            buttons[beingHeld].transform.localPosition = new Vector3(buttons[beingHeld].transform.localPosition.x, 0.015f, buttons[beingHeld].transform.localPosition.z);
            GetComponent<KMBombModule>().HandlePass();
            moduleSolved = true;
            yield break;
        }
        if (!held)
        {
            while (!((int)bomb.GetTime()).ToString().Contains(correctHold.ToString())) { yield return true; }
            buttons[correctBtn].OnInteract();
            if (correctHold == correctRelease)
                yield return null;
        }
        while (!((int)bomb.GetTime()).ToString().Contains(correctRelease.ToString())) { yield return true; }
        buttons[correctBtn].OnInteractEnded();
    }
}

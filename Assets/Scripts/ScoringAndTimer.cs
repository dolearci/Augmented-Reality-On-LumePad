// Author: Daniel Olearčin 2023
// Bachelor thesis AUGMENTED REALITY ON LUME PAD

using LeiaLoft;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class ScoringAndTimer : MonoBehaviour
    {
        private TextMeshProUGUI timerText;

        public static Text ConvergenceText;
        public static Text BaselineText;
        public static TextMeshProUGUI Test12D;
        public static TextMeshProUGUI Test22D;
        public static TextMeshProUGUI Test32D;
        public static TextMeshProUGUI Test13D;
        public static TextMeshProUGUI Test23D;
        public static TextMeshProUGUI Test33D;
        public static Slider BaselineSlider;
        public static Slider TimerSlider;
        public static Slider ConvergenceSlider;
        public static GameObject CalcButton;
        public static Toggle BgToggle;
        public static Toggle TableToggle;
        public static Toggle ShadowToggle;
        public static Toggle Settings3DToggle;
        public static GameObject LeiaD;
        public static GameObject MenuBoardUi;
        public static GameObject ScoreBoardUi;
        public static TextMeshProUGUI ScoreBoardText;
        public static float GameTime = 30.0f;
        public static bool StopTimer;
        public static int Score;

        private void Awake()
        {
            Score = 0;
            StopTimer = false;
            GameTime = 30.0f;
            ButtonsScript.Reset = false;
        }

        // Getting all necessary elements from the scene so scripts can manipulate them
        private void Start()
        {
            StopTimer = false;
            BaselineSlider = GameObject.FindGameObjectWithTag("BaselineSlider").GetComponent<Slider>();
            ConvergenceSlider = GameObject.FindGameObjectWithTag("ConvergenceSlider").GetComponent<Slider>();
            TimerSlider = GameObject.FindGameObjectWithTag("TimerSlider").GetComponent<Slider>();
            TimerSlider.maxValue = GameTime;
            TimerSlider.value = GameTime;
            timerText = GameObject.FindGameObjectWithTag("TimerSliderText").GetComponent<TextMeshProUGUI>();
            ConvergenceText = GameObject.FindGameObjectWithTag("ConvergenceText").GetComponent<Text>();
            BaselineText = GameObject.FindGameObjectWithTag("BaselineText").GetComponent<Text>();
            LeiaD = GameObject.FindGameObjectWithTag("LeiaDisplay");
            ScoreBoardText = GameObject.FindGameObjectWithTag("ScoreBoardText").GetComponent<TextMeshProUGUI>();
            Test12D = GameObject.FindGameObjectWithTag("Test12D").GetComponent<TextMeshProUGUI>();
            Test22D = GameObject.FindGameObjectWithTag("Test22D").GetComponent<TextMeshProUGUI>();
            Test32D = GameObject.FindGameObjectWithTag("Test32D").GetComponent<TextMeshProUGUI>();
            Test13D = GameObject.FindGameObjectWithTag("Test13D").GetComponent<TextMeshProUGUI>();
            Test23D = GameObject.FindGameObjectWithTag("Test23D").GetComponent<TextMeshProUGUI>();
            Test33D = GameObject.FindGameObjectWithTag("Test33D").GetComponent<TextMeshProUGUI>();
            ScoreBoardUi = GameObject.FindGameObjectWithTag("ScoreBoard");
            MenuBoardUi = GameObject.FindGameObjectWithTag("MenuBoard");
            TableToggle = GameObject.FindGameObjectWithTag("TableButton").GetComponent<Toggle>();
            BgToggle = GameObject.FindGameObjectWithTag("BgButton").GetComponent<Toggle>();
            ShadowToggle = GameObject.FindGameObjectWithTag("ShadowButton").GetComponent<Toggle>();
            Settings3DToggle = GameObject.FindGameObjectWithTag("3DsettingsButton").GetComponent<Toggle>();
            CalcButton = GameObject.FindGameObjectWithTag("CalculateButton");
            ScoreBoardUi.SetActive(false);
            MenuBoardUi.SetActive(false);
            BgToggle.gameObject.SetActive(false);
            TableToggle.gameObject.SetActive(false);
            Settings3DToggle.gameObject.SetActive(false);
            BaselineSlider.gameObject.SetActive(false);
            ConvergenceSlider.gameObject.SetActive(false);
        }

        private void Update()
        {
            // Updating a score and timer
            if (ArPlacement.CubeSpawned)
            {
                if (!StopTimer)
                {
                    ScoreBoardUi.SetActive(true);
                    GameTime -= Time.deltaTime;
                    timerText.text = GameTime.ToString("f0");
                    TimerSlider.value = GameTime;
                    if (ArPlacement.ActiveTest == 1)
                    {
                        CalcButton.SetActive(false);
                        if (GameTime <= 0)
                        {
                            StopTimer = true;
                            if (LeiaD.GetComponent<LeiaDisplay>().IsLightfieldModeActualOn())
                                Test13D.text = Score.ToString();
                            else
                                Test12D.text = Score.ToString();
                        }

                        ScoreBoardText.text = "Score: " + Score.ToString();
                    }
                    else if (ArPlacement.ActiveTest == 2)
                    {
                        CalcButton.SetActive(true);
                        ScoreBoardText.text = "Precision: ";
                        if (GameTime <= 0) ButtonsScript.CalculatePrecisionButton();
                    }
                    else if (ArPlacement.ActiveTest == 3)
                    {
                        CalcButton.SetActive(false);
                        if (GameTime <= 0)
                        {
                            if (LeiaD.GetComponent<LeiaDisplay>().IsLightfieldModeActualOn())
                                Test33D.text = Score.ToString();
                            else
                                Test32D.text = Score.ToString();
                            StopTimer = true;
                        }

                        ScoreBoardText.text = "Score: " + Score.ToString();
                    }
                }
                else
                {
                    timerText.text = "0";
                    TimerSlider.value = 0;
                }
            }
        }
    }
}
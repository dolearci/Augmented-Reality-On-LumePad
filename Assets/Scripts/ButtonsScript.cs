// Author: Daniel Olearčin 2023
// Bachelor thesis AUGMENTED REALITY ON LUME PAD

using LeiaLoft;
using UnityEngine;
using UnityEngine.SceneManagement;

// ReSharper disable Unity.InefficientPropertyAccess

namespace Assets.Scripts
{
    public class ButtonsScript : MonoBehaviour
    {
        public static bool Reset;
        public GameObject Room;

        public void ResetButton()
        {
            ScoringAndTimer.Score = 0;
            ScoringAndTimer.StopTimer = true;
            ArPlacement.TestCounter = 0;
            if (ArPlacement.ActiveTest == 1) ScoringAndTimer.GameTime = 30.0f;
            else if (ArPlacement.ActiveTest == 2) ScoringAndTimer.GameTime = 100.0f;
            else if (ArPlacement.ActiveTest == 3) ScoringAndTimer.GameTime = 30.0f;
            ScoringAndTimer.TimerSlider.maxValue = ScoringAndTimer.GameTime;
            ScoringAndTimer.TimerSlider.value = ScoringAndTimer.GameTime;
            Reset = true;
        }

        public void Settings3DButton()
        {
            if (ScoringAndTimer.Settings3DToggle.isOn)
            {
                ScoringAndTimer.ConvergenceSlider.gameObject.SetActive(true);
                ScoringAndTimer.BaselineSlider.gameObject.SetActive(true);
            }
            else
            {
                ScoringAndTimer.ConvergenceSlider.gameObject.SetActive(false);
                ScoringAndTimer.BaselineSlider.gameObject.SetActive(false);
            }
        }

        public void Settings3DSlider()
        {
            LeiaCamera.Instance.BaselineScaling = ScoringAndTimer.BaselineSlider.value;
            LeiaCamera.Instance.ConvergenceDistance = ScoringAndTimer.ConvergenceSlider.value;
            ScoringAndTimer.BaselineText.text =
                "Baseline Scaling = " + LeiaCamera.Instance.BaselineScaling.ToString("F2");
            ScoringAndTimer.ConvergenceText.text =
                "Convergence Distance = " + LeiaCamera.Instance.ConvergenceDistance.ToString("F2");
        }

        public void MenuButton()
        {
            ScoringAndTimer.StopTimer = true;
            ScoringAndTimer.MenuBoardUi.SetActive(true);
            ScoringAndTimer.ScoreBoardUi.SetActive(false);
        }

        public void ExitMenuButton()
        {
            ScoringAndTimer.MenuBoardUi.SetActive(false);
            ScoringAndTimer.ScoreBoardUi.SetActive(true);
        }

        public void ExitGameButton()
        {
            Application.Quit();
        }

        public void TableButton()
        {
            Room.gameObject.SetActive(ScoringAndTimer.TableToggle.isOn);
        }

        public void ButtonBg()
        {
            GameObject.FindGameObjectWithTag("Camera").GetComponent<Camera>().clearFlags = ScoringAndTimer.BgToggle.isOn
                ? CameraClearFlags.Skybox
                : CameraClearFlags.SolidColor;
        }

        public void ButtonShadow()
        {
            if (ScoringAndTimer.ShadowToggle.isOn)
                ArPlacement.LockedPlane.gameObject.SetActive(true);
            else
                ArPlacement.LockedPlane.gameObject.SetActive(false);
        }

        public void Button2D3D()
        {
            if (!ScoringAndTimer.LeiaD.GetComponent<LeiaDisplay>().IsLightfieldModeActualOn())
            {
                ScoringAndTimer.BgToggle.gameObject.SetActive(true);
                ScoringAndTimer.TableToggle.gameObject.SetActive(true);
                ScoringAndTimer.Settings3DToggle.gameObject.SetActive(true);
                GameObject.FindGameObjectWithTag("Camera").AddComponent<LeiaCamera>();
                ScoringAndTimer.BaselineSlider.value = 0.1f;
                ScoringAndTimer.ConvergenceSlider.value = 5.5f;
                Settings3DSlider();
                LeiaDisplay.Instance.DesiredLightfieldMode = LeiaDisplay.LightfieldMode.On;
            }
            else
            {
                LeiaDisplay.Instance.DesiredLightfieldMode = LeiaDisplay.LightfieldMode.Off;
                ScoringAndTimer.BgToggle.gameObject.SetActive(false);
                ScoringAndTimer.TableToggle.gameObject.SetActive(false);
                ScoringAndTimer.Settings3DToggle.gameObject.SetActive(false);
                DestroyImmediate(GameObject.FindGameObjectWithTag("Camera").GetComponent<LeiaCamera>());
                foreach (var o in FindObjectsOfType<GameObject>()) Destroy(o);
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        public static void CalculatePrecisionButton()
        {
            ScoringAndTimer.CalcButton.SetActive(false);
            ScoringAndTimer.StopTimer = true;
            var cubePos = new Vector3(ArPlacement.SpawnedCloneObject.transform.position.x,
                ArPlacement.SpawnedCloneObject.transform.position.y - 0.3f,
                ArPlacement.SpawnedCloneObject.transform.position.z);
            var pos = (0.5f - Vector3.Distance(ArPlacement.SpawnedObject.transform.position,
                cubePos)) * 200f;
            var scale = (0.25f - Mathf.Abs(ArPlacement.SpawnedObject.transform.localScale.x -
                                           ArPlacement.SpawnedCloneObject.transform.localScale.x)) * 400f;
            var rot = Quaternion.Angle(ArPlacement.SpawnedObject.transform.rotation,
                ArPlacement.SpawnedCloneObject.transform.rotation) % 90.0f;
            if (pos < 0) pos = 0;
            if (scale < 0) scale = 0;
            if (rot > 45.0f) rot -= (rot - 45.0f) * 2.0f;

            rot = (45.0f - rot) * 2.22f;
            var precision = (pos + rot + scale) / 3.0f;
            ScoringAndTimer.ScoreBoardText.text = precision.ToString("F2") + "%";
            if (ScoringAndTimer.LeiaD.GetComponent<LeiaDisplay>().IsLightfieldModeActualOn())
                ScoringAndTimer.Test23D.text = precision.ToString("F2") + "%";
            else
                ScoringAndTimer.Test22D.text = precision.ToString("F2") + "%";
        }

        public void HandleInputData(int val)
        {
            if (val == 0)
                ArPlacement.ActiveTest = 1;
            else if (val == 1)
                ArPlacement.ActiveTest = 2;
            else if (val == 2) ArPlacement.ActiveTest = 3;
        }
    }
}
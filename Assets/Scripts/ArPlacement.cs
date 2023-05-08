// Author: Daniel Olearčin 2023
// Bachelor thesis AUGMENTED REALITY ON LUME PAD

using System;
using System.Collections.Generic;
using System.Linq;
using LeiaLoft;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Random = UnityEngine.Random;
using RaycastHit = UnityEngine.RaycastHit;

namespace Assets.Scripts
{
    public class ArPlacement : MonoBehaviour
    {
        public GameObject CubeToSpawn;
        public static GameObject SpawnedObject;
        public GameObject CubeCloneToSpawn;
        public static GameObject SpawnedCloneObject;
        public static ARPlane LockedPlane;
        private List<GameObject> cubes = new List<GameObject>();
        private Pose placementPose;
        private ARRaycastManager raycastManager;
        private ARPlaneManager planeManager;
        private ARPlane pickedPlane;
        private bool placementPoseIsValid;
        private bool spawnSemaphore = true;

        public static bool CubeSpawned;
        public static int ActiveTest = 1;
        public static int TestCounter;

        private void Awake()
        {
            CubeSpawned = false;
            TestCounter = 0;
            placementPoseIsValid = false;
            ActiveTest = 1;
            pickedPlane = null;
            LockedPlane = null;
            SpawnedCloneObject = null;
            SpawnedObject = null;
        }

        private void Start()
        {
            raycastManager = FindObjectOfType<ARRaycastManager>();
            planeManager = FindObjectOfType<ARPlaneManager>();
        }

        private void Update()
        {
            // Locking the first touched AR plane
            if (LockedPlane == null && SpawnedObject == null && placementPoseIsValid && Input.touchCount > 0 &&
                Input.GetTouch(0).phase == TouchPhase.Began && !ButtonsScript.Reset && !CubeSpawned)
            {
                SpawnedObject = Instantiate(CubeToSpawn, placementPose.position, placementPose.rotation);
                CubeSpawned = true;
                LockPlane(pickedPlane);
            }

            // Spawning of green cubes && checking of conditions for test 1 and test 3
            if (LockedPlane != null && !ScoringAndTimer.StopTimer && CubeSpawned)
            {
                if (SpawnedCloneObject == null && TestCounter < 5 && spawnSemaphore)
                {
                    spawnSemaphore = false;
                    SpawnCube(LockedPlane);
                    spawnSemaphore = true;
                }
                else if (ActiveTest == 1)
                {
                    CheckCollision();
                }
                else if (ActiveTest == 3)
                {
                    CheckCubeScales();
                }
            }

            // Destroying of objects after test end
            if (LockedPlane != null && ScoringAndTimer.StopTimer && CubeSpawned)
            {
                if (SpawnedObject != null) Destroy(SpawnedObject);

                if (ActiveTest == 3)
                {
                    foreach (var cube in cubes.Where(cube => cube != null)) Destroy(cube);
                    cubes.Clear();
                }

                if (SpawnedCloneObject != null) Destroy(SpawnedCloneObject);

                CubeSpawned = false;
            }

            // Spawning a red cube after reset
            if (LockedPlane != null && ButtonsScript.Reset && !CubeSpawned && ScoringAndTimer.StopTimer)
            {
                if (ActiveTest != 3)
                    SpawnedObject = Instantiate(CubeToSpawn, placementPose.position, placementPose.rotation);
                CubeSpawned = true;
                ScoringAndTimer.StopTimer = false;
                ButtonsScript.Reset = false;
            }

            if (!ScoringAndTimer.StopTimer && ActiveTest != 3) UpdatePlacementPose();
        }

        // Updating a position for red cube && saving the AR plane
        private void UpdatePlacementPose()
        {
            var screenCenter = GameObject.FindGameObjectWithTag("Camera").GetComponent<Camera>()
                .ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
            var hits = new List<ARRaycastHit>();
            raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon);
            ARRaycastHit? hit = null;
            if (hits.Count > 0)
            {
                placementPoseIsValid = true;
                placementPose = hits[0].pose;

                hit = LockedPlane == null
                    ? hits[0]
                    : hits.SingleOrDefault(x => x.trackableId == LockedPlane.trackableId);
            }

            if (hit.HasValue)
            {
                pickedPlane = planeManager.GetPlane(hit.Value.trackableId);
                var pos = hit.Value.pose.position;
                pos.y += SpawnedObject.transform.localScale.y;
                SpawnedObject.transform.position = pos;
            }
        }

        // Locking the touched AR plane and stopping the generation of new ones
        private void LockPlane(ARPlane keepPlane)
        {
            var arPlane = keepPlane.GetComponent<ARPlane>();
            arPlane.GetComponent<ARPlane>().destroyOnRemoval = false;
            foreach (var plane in planeManager.trackables) plane.gameObject.SetActive(false);
            planeManager.planesChanged += DisableNewPlanes;
            planeManager.enabled = false;
            LockedPlane = arPlane;
        }

        private void DisableNewPlanes(ARPlanesChangedEventArgs args)
        {
            foreach (var plane in args.added) plane.gameObject.SetActive(false);
        }
        
        private static Vector3 RandomInVertice(Vector3 v1, Vector3 v2)
        {
            var u = Random.Range(0.0f, 1.0f);
            var v = Random.Range(0.0f, 1.0f);
            if (v + u > 1)
            {
                v = 1 - v;
                u = 1 - u;
            }

            return v1 * u + v2 * v;
        }

        // Choosing a random place on AR plane for spawning a cube
        private static Vector3 RandomPlaceOnPlane(ARPlane plane)
        {
            var mesh = plane.GetComponent<ARPlaneMeshVisualizer>().mesh;
            var triangles = mesh.triangles;
            var triangle = triangles[Random.Range(0, triangles.Length - 1)] / 3 * 3;
            var vertices = mesh.vertices;
            var randomInTriangle = RandomInVertice(vertices[triangle], vertices[triangle + 1]);
            var randomPoint = plane.transform.TransformPoint(randomInTriangle);

            return randomPoint;
        }

        // Spawning a green cube for each test
        private void SpawnCube(ARPlane plane)
        {
            var rnd = Random.Range(0.05f, 0.2f);
            if (ActiveTest == 1)
            {
                SpawnedCloneObject = Instantiate(CubeCloneToSpawn, RandomPlaceOnPlane(plane),
                    Quaternion.Euler(0f, Random.Range(0.0f, 90.0f), 0f));
                SpawnedCloneObject.transform.localScale = new Vector3(rnd, rnd, rnd);
                SpawnedCloneObject.transform.position = new Vector3(SpawnedCloneObject.transform.position.x,
                    SpawnedCloneObject.transform.position.y + SpawnedCloneObject.transform.localScale.y,
                    SpawnedCloneObject.transform.position.z);
            }
            else if (ActiveTest == 2)
            {
                SpawnedCloneObject = Instantiate(CubeCloneToSpawn, RandomPlaceOnPlane(plane),
                    Quaternion.Euler(0f, Random.Range(0.0f, 90.0f), 0f));
                SpawnedCloneObject.transform.localScale = new Vector3(rnd, rnd, rnd);
                SpawnedCloneObject.transform.position = new Vector3(SpawnedCloneObject.transform.position.x,
                    SpawnedCloneObject.transform.position.y + SpawnedCloneObject.transform.localScale.y + 0.3f,
                    SpawnedCloneObject.transform.position.z);
            }
            else if (ActiveTest == 3)
            {
                for (var i = 0; i < 5; i++)
                {
                    cubes.Add(Instantiate(CubeCloneToSpawn, RandomPlaceOnPlane(plane), Quaternion.Euler(0f, 0f, 0f)));
                }

                SpawnedCloneObject = cubes[0];
                while (Vector3.Distance(cubes[0].transform.position, cubes[1].transform.position) < 0.4f ||
                       Vector3.Distance(cubes[0].transform.position, cubes[2].transform.position) < 0.4f ||
                       Vector3.Distance(cubes[0].transform.position, cubes[3].transform.position) < 0.4f ||
                       Vector3.Distance(cubes[0].transform.position, cubes[4].transform.position) < 0.4f ||
                       Vector3.Distance(cubes[1].transform.position, cubes[2].transform.position) < 0.4f ||
                       Vector3.Distance(cubes[1].transform.position, cubes[3].transform.position) < 0.4f ||
                       Vector3.Distance(cubes[1].transform.position, cubes[4].transform.position) < 0.4f ||
                       Vector3.Distance(cubes[2].transform.position, cubes[3].transform.position) < 0.4f ||
                       Vector3.Distance(cubes[2].transform.position, cubes[4].transform.position) < 0.4f ||
                       Vector3.Distance(cubes[3].transform.position, cubes[4].transform.position) < 0.4f)
                {
                    cubes[0].transform.position = RandomPlaceOnPlane(plane);
                    cubes[1].transform.position = RandomPlaceOnPlane(plane);
                    cubes[2].transform.position = RandomPlaceOnPlane(plane);
                    cubes[3].transform.position = RandomPlaceOnPlane(plane);
                    cubes[4].transform.position = RandomPlaceOnPlane(plane);
                }

                var scales = new List<float>();
                foreach (var cube in cubes)
                {
                    do
                    {
                        rnd = Random.Range(0.04f, 0.16f);
                    } while (scales.Contains(rnd));

                    cube.transform.localScale = new Vector3(rnd, rnd, rnd);
                    cube.transform.position = new Vector3(cube.transform.position.x,
                        cube.transform.position.y + cube.transform.localScale.y,
                        cube.transform.position.z);
                    scales.Add(rnd);
                }
            }
        }

        // Checking a right position of red cube in test 1 && updating a score
        private void CheckCollision()
        {
            if (SpawnedObject != null && SpawnedCloneObject != null)
                if (Vector3.Distance(SpawnedObject.transform.position, SpawnedCloneObject.transform.position) < 0.04f
                    && (Quaternion.Angle(SpawnedObject.transform.rotation, SpawnedCloneObject.transform.rotation) %
                        90.0f < 4.0f
                        || Quaternion.Angle(SpawnedObject.transform.rotation, SpawnedCloneObject.transform.rotation) %
                        90.0f > 85.0f)
                    && Mathf.Abs(SpawnedObject.transform.localScale.x - SpawnedCloneObject.transform.localScale.x) <
                    0.03f)
                {
                    ScoringAndTimer.Score++;
                    Destroy(SpawnedCloneObject);
                }
        }

        // Checking Cube scales in test 3 && updating a score
        private void CheckCubeScales()
        {
            if (TestCounter == 5)
            {
                TestCounter = 0;
                ScoringAndTimer.StopTimer = true;
                if (ScoringAndTimer.LeiaD.GetComponent<LeiaDisplay>().IsLightfieldModeActualOn())
                    ScoringAndTimer.Test33D.text = ScoringAndTimer.Score.ToString();
                else
                    ScoringAndTimer.Test32D.text = ScoringAndTimer.Score.ToString();
            }

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                var raycast = GameObject.FindGameObjectWithTag("Camera").GetComponent<Camera>()
                    .ScreenPointToRay(Input.GetTouch(0).position);
                RaycastHit raycastHit;
                if (Physics.Raycast(raycast, out raycastHit))
                    if (raycastHit.collider.CompareTag("Cube"))
                    {
                        TestCounter++;
                        var cubeScales = cubes.Select(cube => cube.transform.localScale.x).ToList();
                        cubeScales.Sort((a, b) => a.CompareTo(b));
                        for (var i = 0; i < 5; i++)
                            if (Math.Abs(cubeScales[i] - raycastHit.collider.transform.localScale.x) < 0.005f)
                            {
                                ScoringAndTimer.Score += i;
                                break;
                            }

                        foreach (var cube in cubes.Where(cube => cube != null)) Destroy(cube);
                        cubes.Clear();
                    }
            }
        }
    }
}
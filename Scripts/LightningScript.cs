using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using ShipPlusAMod;

namespace ShipPlusA.Scripts
{

    public class LightningScript : MonoBehaviour
    {
        
        public static float offsetScale = 1.25f;
        public Vector3 startLocation;
        public Vector3 endLocation;
        public static float duration = 6f;
        public int amount = 4;
        public List<Transform> lineObjects;
        public List<LineRenderer> lineRenderers;
        public static float noiseScale = 20f;
        public bool shootingAlready = false;

        void Start()
        {
            lineObjects = new List<Transform>();
            lineRenderers = new List<LineRenderer>();

            for (int i = 0; i < amount; i++)
            {
                GameObject lineObject = new GameObject("LightningLine" + i.ToString());
                lineObject.transform.SetParent(transform);
                lineObjects.Add(lineObject.transform);

                LineRenderer ln = lineObject.AddComponent<LineRenderer>();
                ln.positionCount = 2;
                ln.useWorldSpace = true;
                lineRenderers.Add(ln);

                if (i % 2 == 0)
                {
                    SetLineProperties(ln, Color.white);
                }
                else
                {
                    SetLineProperties(ln, Color.yellow);
                }
            }

            gameObject.SetActive(false);
        }

        void SetLineProperties(LineRenderer ln, Color color)
        {
            Shader hdrpShader = Shader.Find("HDRP/Lit");
            ln.material = new Material(hdrpShader);
            ln.startWidth = 0.04f;
            ln.endWidth = 0.01f;
            ln.startColor = color;
            ln.endColor = color;
            ln.material.SetColor("_BaseColor", color);
            ln.material.SetFloat("_EmissiveIntensity", 1f);
            ln.material.EnableKeyword("_EMISSION");
        }

        public void updateLoc(Vector3 loc1, Vector3 loc2)
        {
            startLocation = loc1;
            endLocation = loc2;
        }

        public void ShootLightning(Vector3 loc1, Vector3 loc2)
        {
            startLocation = loc1;
            endLocation = loc2;
            duration = ShipModBase.upgrades[ShipModBase.upgradeLevel].time;
            int dis = Mathf.RoundToInt(Vector3.Distance(loc1, loc2));

            for (int i = 0; i < amount; i++)
            {
                lineRenderers[i].positionCount = dis * 2;
            }

            StartCoroutine(ShootLightningCoroutine(dis));
        }

        IEnumerator ShootLightningCoroutine(int dis)
        {
            float startTime = Time.time;
            Vector3[][] positions = new Vector3[amount][];

            for (int j = 0; j < amount; j++)
            {
                LineRenderer ln = lineRenderers[j];

                positions[j] = new Vector3[ln.positionCount];
                for (int i = 0; i < ln.positionCount; i++)
                {
                    positions[j][i] = Vector3.Lerp(startLocation, endLocation, i / (float)(ln.positionCount - 1));
                }

                ln.SetPositions(positions[j]);
                UpdateLineObjects(positions[j], lineObjects[j]);
            }

            while (Time.time - startTime < duration)
            {
                for (int j = 0; j < amount; j++)
                {
                    LineRenderer ln = lineRenderers[j];
                    for (int i = 0; i < ln.positionCount; i++)
                    {
                        float noise = Mathf.PerlinNoise(i * noiseScale, Time.time * noiseScale) + Mathf.Sin(j * 0.1f);
                        float verticalOffset = UnityEngine.Random.Range(-0.2f, 0.2f);
                        positions[j][i] = Vector3.Lerp(startLocation, endLocation, i / (float)(ln.positionCount - 1)) + new Vector3(0, noise + verticalOffset, 0);
                    }

                    ln.SetPositions(positions[j]);
                    UpdateLineObjects(positions[j], lineObjects[j]);
                }

                yield return new WaitForSeconds(0.001f);
            }
            for (int j = 0; j < amount; j++)
            {
                Transform lineObject = lineObjects[j];
                for (int i = 0; i < lineObject.childCount; i++)
                {
                    Destroy(lineObject.GetChild(i).gameObject);
                }
            }
            yield return new WaitForSeconds(0.001f);
            gameObject.SetActive(false);
        }

        void UpdateLineObjects(Vector3[] positions, Transform lineObject)
        {
            int childCount = lineObject.childCount;

            if (childCount != positions.Length)
            {
                while (lineObject.childCount < positions.Length)
                {
                    GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    Destroy(cylinder.GetComponent<Collider>());
                    cylinder.transform.SetParent(lineObject);
                }

                while (lineObject.childCount > positions.Length)
                {
                    Transform lastChild = lineObject.GetChild(lineObject.childCount - 1);
                    Destroy(lastChild.gameObject);
                }
            }

            for (int i = 0; i < positions.Length; i++)
            {
                Transform cylinder = lineObject.GetChild(i);
                cylinder.position = positions[i];

                if (i < positions.Length - 1)
                {
                    cylinder.LookAt(positions[i + 1]);
                }
            }
        }
    }
}

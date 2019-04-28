﻿using Bolt;
using BuilderCore;
using ChampionsOfForest.Enemies;
using System.Collections;
using TheForest.Utils;
using UnityEngine;

namespace ChampionsOfForest.Effects
{
    public class MagicArrow : MonoBehaviour
    {
        private static Material material;
        public static void Create(Vector3 pos, Vector3 dir, float Damage, string CasterID, float debuffDuration, bool doubleSlow, bool dmgdebuff)
        {
            MagicArrow a = CreateEffect(pos, dir,dmgdebuff,debuffDuration);
            BoxCollider col = a.gameObject.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(0.4f, 0.4f, 1.2f);
            a.Damage = Damage;
            a.casterID = CasterID;
            a.DebuffDuration = debuffDuration;
            a.GiveDoubleSlow = doubleSlow;
            a.GiveDmgDebuff = dmgdebuff;
        }
        public static MagicArrow CreateEffect(Vector3 pos, Vector3 dir, bool debuff,float duration)
        {
            GameObject go = new GameObject("__MagicArrow__");
            go.transform.position = pos;
            go.transform.rotation = Quaternion.LookRotation(dir);
            go.AddComponent<Rigidbody>().isKinematic = true;
            if (!ModSettings.IsDedicated)
            {
                go.AddComponent<MeshFilter>().mesh = Res.ResourceLoader.instance.LoadedMeshes[113];
                if (material == null)
                {
                    material = Core.CreateMaterial(new BuildingData() { EmissionColor = new Color(0, 1, 0.287f), Metalic = 1, Smoothness = 1 });
                }
                go.AddComponent<MeshRenderer>().material = material;
            }
            MagicArrow a = go.AddComponent<MagicArrow>();
            a.GiveDmgDebuff = debuff;
            a.DebuffDuration = duration;

            var light = go.AddComponent<Light>();
            light.shadowStrength = 1;
            light.shadows = LightShadows.Hard;
            light.type = LightType.Point;
            light.range = 18;
            light.color = new Color(0.2f, 1f, 0.2f);
            light.intensity = 0.6f;
            Destroy(go, duration);

            return a;
        }


        public string casterID;

        public float Damage;
        public float DebuffDuration;
        public bool GiveDmgDebuff;
        public bool GiveDoubleSlow;

        private bool setupComplete = false;
        private readonly float speed = 50;

        public IEnumerator Animate()
        {
            transform.localScale = new Vector3(4, 4, 0);

            yield return null;
            while (transform.localScale.z < 4)
            {
                transform.localScale += Vector3.forward * Time.deltaTime / 2;
            }
            yield return new WaitForSeconds(0.7f);
            setupComplete = true;

        }

        private void Start()
        {
            setupComplete = false;
            StartCoroutine(Animate());
            Destroy(gameObject, 7);
        }

        private void Update()
        {
            transform.Rotate(transform.forward * 60 * Time.deltaTime, Space.World);
            if (setupComplete)
            {
                transform.Translate(Vector3.forward * speed * Time.deltaTime);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!setupComplete)
            {

            }
          
                if (other.gameObject.CompareTag("enemyCollide"))
                {
                if (!GameSetup.IsMpClient) { 
                    EnemyProgression prog = other.GetComponentInParent<EnemyProgression>();
                        DamageMath.DamageClamp(Damage, out int d, out int a);
                    if (prog != null)
                    {
                        for (int i = 0; i < a; i++)
                        {
                        prog.HitMagic(d);
                        }
                        float slowAmount = 0.35f;
                        if (GiveDoubleSlow)
                        {
                            slowAmount *= 2;
                        }

                        prog.Slow(41, 1 - slowAmount, DebuffDuration);
                        if (GiveDmgDebuff)
                        {
                            prog.DmgTakenDebuff(41, 1.15f, DebuffDuration);
                        }
                    }
                    else
                    {
                        other.SendMessageUpwards("HitMagic", d, SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
        }
    }
}

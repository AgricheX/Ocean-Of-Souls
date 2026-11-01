using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OceanOfSouls
{
    internal sealed class SoulAmbience : MonoBehaviour
    {
        private const float Width = 214f;
        private const int StarCount = 16;
        private const int WispCount = 5;

        private sealed class Particle
        {
            public RectTransform Rect;
            public Image Image;
            public float X;
            public float Y;
            public float Speed;
            public float Phase;
            public float Rate;
            public bool Wisp;
        }

        private readonly List<Particle> particles = new List<Particle>();

        public static SoulAmbience Create(Transform parent)
        {
            GameObject root = new GameObject("OceanOfSoulsAmbience", typeof(RectTransform), typeof(LayoutElement));
            root.transform.SetParent(parent, false);
            root.GetComponent<LayoutElement>().ignoreLayout = true;
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(Width, 0f);
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
            root.transform.SetAsFirstSibling();

            SoulAmbience ambience = root.AddComponent<SoulAmbience>();
            ambience.Populate();
            return ambience;
        }

        private void Populate()
        {
            System.Random random = new System.Random(77);
            for (int i = 0; i < StarCount; i++)
            {
                if (PantheonArt.Star == null) break;
                particles.Add(NewParticle(PantheonArt.Star, new Vector2(5f, 5f), false, random));
            }
            for (int i = 0; i < WispCount; i++)
            {
                if (PantheonArt.Wisp == null) break;
                particles.Add(NewParticle(PantheonArt.Wisp, new Vector2(7f, 11f), true, random));
            }
        }

        private Particle NewParticle(Sprite sprite, Vector2 size, bool wisp, System.Random random)
        {
            GameObject go = new GameObject(wisp ? "Wisp" : "Star", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;

            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;

            Particle particle = new Particle
            {
                Rect = rect,
                Image = image,
                X = (float)random.NextDouble(),
                Y = (float)random.NextDouble(),
                Speed = 0.025f + (float)random.NextDouble() * 0.04f,
                Phase = (float)random.NextDouble() * 6.28f,
                Rate = 0.8f + (float)random.NextDouble() * 1.6f,
                Wisp = wisp
            };
            Place(particle, particle.X);
            return particle;
        }

        private static void Place(Particle particle, float x)
        {
            Vector2 anchor = new Vector2(x, particle.Y);
            particle.Rect.anchorMin = anchor;
            particle.Rect.anchorMax = anchor;
            particle.Rect.anchoredPosition = Vector2.zero;
        }

        private void Update()
        {
            float time = Time.unscaledTime;
            float delta = Time.unscaledDeltaTime;
            foreach (Particle particle in particles)
            {
                float alpha;
                float x = particle.X;
                if (particle.Wisp)
                {
                    particle.Y += particle.Speed * delta;
                    if (particle.Y > 1.05f) particle.Y = -0.05f;
                    x += 0.02f * Mathf.Sin(time * 0.8f + particle.Phase);
                    alpha = Mathf.Clamp01(Mathf.Sin(Mathf.Clamp01(particle.Y) * Mathf.PI)) * 0.85f;
                }
                else
                {
                    alpha = 0.2f + 0.6f * (0.5f + 0.5f * Mathf.Sin(time * particle.Rate + particle.Phase));
                }

                Place(particle, x);
                Color color = particle.Image.color;
                color.a = alpha;
                particle.Image.color = color;
            }
        }
    }
}

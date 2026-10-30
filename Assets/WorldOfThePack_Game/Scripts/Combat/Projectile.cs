using UnityEngine;

namespace WorldOfThePack.Game
{
    public class Projectile : MonoBehaviour
    {
        private Combatant target;
        private float damage;
        private string hitVfx;
        private float speed = 7.5f;

        public static void SpawnProjectile(Vector3 startPos, Combatant target, float damage, string hitVfx)
        {
            GameObject go = new GameObject("Projectile_" + hitVfx);
            go.transform.position = startPos;
            Projectile proj = go.AddComponent<Projectile>();
            proj.Init(target, damage, hitVfx);
        }

        public void Init(Combatant target, float damage, string hitVfx)
        {
            this.target = target;
            this.damage = damage;
            this.hitVfx = hitVfx;

            // Visual representation
            SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = GetProjectileSprite();
            sr.color = hitVfx == "StarShower" ? new Color(1f, 0.9f, 0.2f) : new Color(0.4f, 0.95f, 0.5f);
            sr.sortingOrder = 500;
            transform.localScale = new Vector3(0.5f, 0.5f, 1f);

            Destroy(gameObject, 3f);
        }

        private void Update()
        {
            if (target == null || !target.IsAlive)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 dir = (target.Position + Vector3.up * 0.3f - transform.position).normalized;
            transform.position += dir * (speed * Time.deltaTime);

            if (Vector2.Distance(transform.position, target.Position) <= 0.35f)
            {
                target.TakeDamage(damage);
                if (!string.IsNullOrEmpty(hitVfx))
                {
                    CombatVFXPool.SpawnEffect(hitVfx, target.Position + Vector3.up * 0.2f);
                }
                Destroy(gameObject);
            }
        }

        private static Sprite cachedProjSprite;
        private static Sprite GetProjectileSprite()
        {
            if (cachedProjSprite != null) return cachedProjSprite;
            int size = 16;
            Texture2D tex = new Texture2D(size, size);
            Vector2 center = new Vector2(size / 2f, size / 2f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (Vector2.Distance(new Vector2(x, y), center) <= size / 2f) tex.SetPixel(x, y, Color.white);
                    else tex.SetPixel(x, y, Color.clear);
                }
            }
            tex.Apply();
            cachedProjSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
            return cachedProjSprite;
        }
    }
}

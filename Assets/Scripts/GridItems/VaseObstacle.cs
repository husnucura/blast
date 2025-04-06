using System.Collections;
using UnityEngine;

public class VaseObstacle : Obstacle
{
    public VaseObstacle(GameObject gameObject, int x, int y) : base(gameObject, ItemType.Vase, x, y)
    {
        health = 2;
    }
    public override bool DealDamage(int damage, DamageSource damageSource, GridState gridState)
    {
        bool result = base.DealDamage(damage, damageSource, gridState);
        if (result)
        {
            gridState.vaseTarget--;
        }
        return result;
    }

    public override IEnumerator DealDamageAnimation()
    {
        if (gameObject == null)
            yield break;

        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        GridItemComponent gridItemComponent = gameObject.GetComponent<GridItemComponent>();

        if (spriteRenderer == null || gridItemComponent == null)
            yield break;

        float fadeDuration = 0.2f;
        float t = 0f;

        Color originalColor = spriteRenderer.color;

        // Fade out
        while (t < fadeDuration)
        {
            if (spriteRenderer == null) yield break;

            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            yield return null;
        }

        // Change sprite
        if (spriteRenderer == null || gridItemComponent.Sprites.Length < 2)
            yield break;

        spriteRenderer.sprite = gridItemComponent.Sprites[1];

        // Fade in
        t = 0f;
        while (t < fadeDuration)
        {
            if (spriteRenderer == null) yield break;

            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            yield return null;
        }
    }


    public override bool IsFallable()
    {
        return true;
    }
}

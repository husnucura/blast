using System.Collections;
using UnityEngine;

public class VaseObstacle : Obstacle
{
    public VaseObstacle(GameObject gameObject, int x, int y) : base(gameObject, ItemType.Vase, x, y)
    {
        health = 2;
    }

    public override IEnumerator DealDamageAnimation()
    {
        if (gameObject == null)
            yield return null;
        else
        {
            SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            GridItemComponent gridItemComponent = gameObject.GetComponent<GridItemComponent>();

            // Fade out the current sprite
            float fadeDuration = 0.2f;
            Color currentColor = spriteRenderer.color;
            currentColor.a = 0;
            float t = 0f;

            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, Mathf.Lerp(0, 1, t / fadeDuration));
                yield return null;
            }

            spriteRenderer.sprite = gridItemComponent.Sprites[1];

            // Fade in the new sprite
            t = 0f;
            currentColor.a = 0;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, Mathf.Lerp(0, 1, t / fadeDuration));
                yield return null;
            }
        }
    }

    public override bool IsFallable()
    {
        return true;
    }
}

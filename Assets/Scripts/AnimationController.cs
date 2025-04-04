using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class AnimationController : MonoBehaviour
{
    private Queue<List<IEnumerator>> animationQueue = new Queue<List<IEnumerator>>();
    private bool isPlaying = false;

    private void OnEnable()
    {
        GridEvents.OnItemsDestroyed += HandleItemsDestroyed;
        GridEvents.OnItemsFall += HandleItemsFall;
        GridEvents.OnNewItemsCreated += HandleNewItems;
        GridEvents.OnNewRocketCreated += HandeNewRocket;
    }


    private void OnDisable()
    {
        GridEvents.OnItemsDestroyed -= HandleItemsDestroyed;
        GridEvents.OnItemsFall -= HandleItemsFall;
        GridEvents.OnNewItemsCreated -= HandleNewItems;
        GridEvents.OnNewRocketCreated -= HandeNewRocket;
    }

    private void HandleItemsDestroyed(List<GridItem> gridItems)
    {
        Debug.Log("Destroyed: " + gridItems.Count);
        var animations = new List<IEnumerator>();
        foreach (var item in gridItems)
            animations.Add(DestructionSequence(item));
        EnqueueAnimationGroup(animations);
    }
    private void HandeNewRocket(NewRocketData data)
    {
        var animations = new List<IEnumerator>();

        foreach (var item in data.GridItems)
        {
            animations.Add(MoveToCenterAndDestroy(item, data.GridPos));
        }

        // Animate the rocket appearing after the others combine

        EnqueueAnimationGroup(animations);
        EnqueueAnimationGroup(new List<IEnumerator> { SpawnRocketAnimation(data.Rocket) });
    }

    private IEnumerator MoveToCenterAndDestroy(GridItem item, Vector2Int targetGridPos, float duration = 0.2f)
    {
        GameObject obj = item.gameObject;
        Vector3 start = obj.transform.position;
        Vector3 end = GridPositionCalculator.Instance.GetWorldPosition(targetGridPos.x, targetGridPos.y);

        float t = 0f;
        while (t < duration)
        {
            obj.transform.position = Vector3.Lerp(start, end, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = end;
        Destroy(obj);
    }

    private IEnumerator SpawnRocketAnimation(GridItem rocket, float duration = 0.1f)
    {
        GameObject obj = rocket.gameObject;
        obj.transform.localScale = Vector3.zero;
        obj.SetActive(true);

        float t = 0f;
        while (t < duration)
        {
            obj.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        obj.transform.localScale = Vector3.one;
    }


    private void HandleItemsFall(List<FallData> fallingItems)
    {
        var animations = new List<IEnumerator>();
        foreach (var data in fallingItems)
            animations.Add(FallAnimation(data.GridItem, data.FallDistance));
        EnqueueAnimationGroup(animations);
    }

    private void HandleNewItems(List<NewItemData> newItems)
    {
        var animations = new List<IEnumerator>();
        foreach (var data in newItems)
        {
            animations.Add(FallAnimation(data.GridItem, data.FallDistance));
        }
        EnqueueAnimationGroup(animations);
    }

    private void EnqueueAnimationGroup(List<IEnumerator> animations)
    {
        animationQueue.Enqueue(animations);
        if (!isPlaying)
        {
            StartCoroutine(ProcessQueue());
        }
    }

    private IEnumerator ProcessQueue()
    {
        isPlaying = true;

        while (animationQueue.Count > 0)
        {
            var group = animationQueue.Dequeue();
            yield return StartCoroutine(PlayAnimationsInParallel(group));
        }

        isPlaying = false;
    }

    private IEnumerator PlayAnimationsInParallel(List<IEnumerator> animations)
    {
        List<Coroutine> running = new List<Coroutine>();
        foreach (var anim in animations)
        {
            running.Add(StartCoroutine(anim));
        }
        foreach (var coroutine in running)
        {
            yield return coroutine;
        }
    }

    private IEnumerator DestroyAnimation(GridItem item, float duration = 0.3f)
    {
        Debug.Log("Destroy anim");
        GameObject obj = item.gameObject;
        Vector3 start = obj.transform.localScale;
        float t = 0f;
        while (t < duration)
        {
            obj.transform.localScale = Vector3.Lerp(start, Vector3.zero, t / duration);
            t += Time.deltaTime;
            yield return null;
        }
        obj.transform.localScale = Vector3.zero;
        Destroy(obj);
    }
    private IEnumerator DestructionSequence(GridItem item)
    {
        CreateParticles(item);
        Destroy(item.gameObject);
        return null;
    }

    private void CreateParticles(GridItem item, float explosionForce = 5f, float fadeDuration = 2.5f)
    {
        Sprite[] particleSprites = GetParticleSprites(item);
        int target = 10;
        while (target > 0)
        {
            foreach (Sprite particleSprite in particleSprites)
            {
                target--;
                GameObject particle = new GameObject("Particle");
                particle.transform.position = item.gameObject.transform.position;
                particle.transform.localScale = particle.transform.localScale * 0.3f;
                SpriteRenderer sr = particle.AddComponent<SpriteRenderer>();
                sr.sprite = particleSprite;
                Vector2 dir = Random.insideUnitCircle.normalized;
                float force = Random.Range(explosionForce * 0.8f, explosionForce * 1.2f);
                StartCoroutine(MoveParticle(particle.transform, dir * force, fadeDuration));
            }
        }
    }

    private Sprite[] GetParticleSprites(GridItem item)
    {
        Sprite[] res = item.gameObject.GetComponent<GridItemComponent>().Sprites;
        if (item.IsCube())
        {
            res = res.Skip(2).Take(1).ToArray();
        }
        else if (item.IsObstacle())
        {
            if (item is VaseObstacle)
            {
                res = res.Skip(2).Take(3).ToArray();
            }
            else
            {
                res = res.Skip(1).Take(3).ToArray();
            }

        }
        return res;
    }

    private IEnumerator MoveParticle(Transform particle, Vector2 direction, float fadeDuration)
    {
        float elapsed = 0f;
        Color startColor = particle.GetComponent<SpriteRenderer>().color;

        while (elapsed < fadeDuration)
        {
            particle.position += (Vector3)direction * Time.deltaTime;
            particle.GetComponent<SpriteRenderer>().color =
                Color.Lerp(startColor, Color.clear, elapsed / fadeDuration);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(particle.gameObject);
    }

    private IEnumerator FallAnimation(GridItem item, int dY)
    {
        item.gameObject.SetActive(true);

        if (dY <= 0) yield break;

        GameObject obj = item.gameObject;
        Vector3 start = obj.transform.position;
        Vector2Int startPos = GridPositionCalculator.Instance.GetGridPosition(start);
        Vector3 end = GridPositionCalculator.Instance.GetWorldPosition(startPos.x, startPos.y - dY);

        float duration = 0.1f * dY;
        float t = 0f;
        while (t < duration)
        {
            obj.transform.position = Vector3.Lerp(start, end, t / duration);
            t += Time.deltaTime;
            yield return null;
        }
        obj.transform.position = end;
    }
}

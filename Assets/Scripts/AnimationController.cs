using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            animations.Add(DestroyAnimation(item));
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
        EnqueueAnimationGroup(new List<IEnumerator>{SpawnRocketAnimation(data.Rocket)});
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

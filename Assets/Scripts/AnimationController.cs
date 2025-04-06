using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Android.Gradle.Manifest;
using UnityEditor.Rendering;
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
        GridEvents.OnNewRocketCreated += HandleNewRocket;
        GridEvents.OnRocketBlastStarted += HandlePairRocketBlastStarted;
        GridEvents.OnRocketBlastCombo += HandleRocketBlastCombo;
    }


    private void OnDisable()
    {
        GridEvents.OnItemsDestroyed -= HandleItemsDestroyed;
        GridEvents.OnItemsFall -= HandleItemsFall;
        GridEvents.OnNewItemsCreated -= HandleNewItems;
        GridEvents.OnNewRocketCreated -= HandleNewRocket;
        GridEvents.OnRocketBlastStarted -= HandlePairRocketBlastStarted;
        GridEvents.OnRocketBlastCombo -= HandleRocketBlastCombo;
    }
    private void HandleRocketBlastCombo(RocketBlastCombo combo)
    {
        List<IEnumerator> animations = new List<IEnumerator>
        {
            RocketComboSetupAnimation(combo)
        };
        var rocketAnimations = PlayAnimationsInParallel(RocketAnimations(combo.rocketBlastDatas));
        animations.Add(rocketAnimations);
        EnqueueAnimationGroup(new List<IEnumerator> { PlayAnimationsInSequence(animations) });
    }
    private IEnumerator RocketComboSetupAnimation(RocketBlastCombo combo)
    {
        var centerAnimations = new List<IEnumerator>();
        var animations = new List<IEnumerator>();
        List<GridItem> gridItems = new List<GridItem>();
        foreach (var item in combo.GridItems)
        {
            if (item is Rocket)
                centerAnimations.Add(MoveToCenterAndDestroy(item, combo.center));
            else
            {
                gridItems.Add(item);

            }
        }
        animations.Add(PlayAnimationsInParallel(centerAnimations));
        var destructionAnimations = new List<IEnumerator>();
        animations.Add(PlayAnimationsInParallel(DestructionSequence(gridItems)));
        return PlayAnimationsInSequence(animations);

    }

    private void HandlePairRocketBlastStarted(List<RocketBlastData> datas)
    {
        StartCoroutine(PlayAnimationsInParallel(RocketAnimations(datas)));
    }
    private List<IEnumerator> RocketAnimations(List<RocketBlastData> datas)
    {
        List<IEnumerator> animations = new List<IEnumerator>();
        foreach (RocketBlastData data in datas)
        {
            animations.Add(HandleSingleRocket(data));
        }
        return animations;
    }


    private IEnumerator HandleSingleRocket(RocketBlastData data)
    {
        if (data.originalRocket != null)
            Destroy(data.originalRocket.gameObject);
        GameObject rocket = GridItemFactory.Instance.CreateSplitRocket(data.startPos, data.direction);
        rocket.SetActive(true);
        Vector3 squareMove = new Vector3(data.direction.x, data.direction.y, 0f) * GridPositionCalculator.squareWidth;
        Vector3 startPos = rocket.transform.position;

        Queue<AnimationType> animationQueue = data.animationQueue;
        Queue<GridItem> itemQueue = data.itemQueue;

        float duration = 0.05f;
        float t = 0f;
        Vector3 endPos;
        while (animationQueue.Count > 0)
        {
            t = 0f;
            endPos = rocket.transform.position + squareMove;
            AnimationType anim = animationQueue.Dequeue();
            GridItem item = itemQueue.Dequeue();

            if (item != null)
            {
                StartCoroutine(PlayRocketItemInteractionAnimation(anim, item));
            }
            while (t < duration)
            {

                AnimateSmokeEffect(7, new Vector3(rocket.transform.position.x, rocket.transform.position.y, -80), squareMove, 0.5f);
                rocket.transform.position = Vector3.Lerp(startPos, endPos, t / duration);
                t += Time.deltaTime;
                yield return null;
            }

            rocket.transform.position = endPos;
            startPos = endPos;


        }
        t = 0f;
        endPos = rocket.transform.position + squareMove;
        while (t < duration)
        {
            rocket.transform.position = Vector3.Lerp(startPos, endPos, t / duration);
            t += Time.deltaTime;
            yield return null;
        }
        Destroy(rocket);
        RocketHandler.DecrementRockets();
    }

    private void AnimateSmokeEffect(int num, Vector3 startPos, Vector3 direction, float duration)
    {
        while (num-- > 0)
            StartCoroutine(AnimateSmokeEffect(startPos, direction, duration));

    }
    private IEnumerator AnimateSmokeEffect(Vector3 startPos, Vector3 direction, float duration)
    {
        GameObject smokeEffect = GridItemFactory.Instance.CreateSmokeOrStar(startPos, 0.7f);
        float elapsedTime = 0f;
        float fadeDuration = duration * 0.5f;
        Color startColor = smokeEffect.GetComponent<SpriteRenderer>().color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f); // Transparent color for fading out
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        Vector3 moveDirection = new Vector3(randomDir.x, randomDir.y, 0f);
        while (elapsedTime < duration)
        {
            smokeEffect.transform.position += moveDirection * Time.deltaTime * GridPositionCalculator.squareWidth;

            if (elapsedTime > fadeDuration)
            {
                smokeEffect.GetComponent<SpriteRenderer>().color = Color.Lerp(startColor, targetColor, (elapsedTime - fadeDuration) / fadeDuration);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Destroy(smokeEffect);
    }



    private IEnumerator PlayRocketItemInteractionAnimation(AnimationType type, GridItem item)
    {
        switch (type)
        {
            case AnimationType.Destruction:
                CreateParticles(item);
                Destroy(item.gameObject);
                break;

            case AnimationType.Damage:
                StartCoroutine(DamageAnimation(item as Obstacle));
                break;

            case AnimationType.AnotherRocket:
                GridEvents.TriggerRocketBlastChainStarted(item.GridPosition);
                break;

            case AnimationType.Null:
            default:
                break;
        }

        yield return null;
    }



    private void HandleItemsDestroyed(List<GridItem> gridItems)
    {
        Debug.Log("Destroyed: " + gridItems.Count);
        var animations = new List<IEnumerator>();
        foreach (var item in gridItems)
            animations.Add(DestructionSequence(item));
        EnqueueAnimationGroup(animations);
    }
    private void HandleNewRocket(NewRocketData data)
    {
        var animations = new List<IEnumerator>();

        foreach (var item in data.GridItems)
        {
            animations.Add(MoveToCenterAndDestroy(item, data.GridPos));
        }


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
            animations.Add(FallAnimation(0, data.GridItem, data.FallDistance));
        EnqueueAnimationGroup(animations);
    }

    private void HandleNewItems(List<NewItemData> newItems)
    {
        var animations = new List<IEnumerator>();
        int delay = 0;
        foreach (var data in newItems)
        {
            animations.Add(FallAnimation(delay, data.GridItem, data.FallDistance));
        }
        EnqueueAnimationGroup(animations);
        EnqueueAnimationGroup(new List<IEnumerator> { GridUpdatefinished() });
    }
    private IEnumerator GridUpdatefinished()
    {
        GridEvents.TriggerGridUpdateAnimationFinished();
        yield return null;
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
    private IEnumerator PlayAnimationsInSequence(List<IEnumerator> animations)
    {
        foreach (var anim in animations)
        {
            yield return StartCoroutine(anim);
        }
    }
    private IEnumerator DamageAnimation(Obstacle item)
    {
        if (item != null && item.gameObject != null)
            yield return StartCoroutine(item.DealDamageAnimation());
        yield return null;

    }
    private IEnumerator DestructionSequence(GridItem item)
    {
        CreateParticles(item);
        Destroy(item.gameObject);
        yield return null;
    }
    private List<IEnumerator> DestructionSequence(List<GridItem> items)
    {
        List<IEnumerator> animations = new List<IEnumerator>();
        foreach (GridItem gridItem in items)
        {
            animations.Add(DestructionSequence(gridItem));
        }
        return animations;
    }

    private void CreateParticles(GridItem item, float explosionForce = 5f, float fadeDuration = 1.0f)
    {
        Sprite[] particleSprites = GetParticleSprites(item);
        int target = 7;

        Vector3 startPos = item.gameObject.transform.position;

        while (target > 0)
        {
            foreach (Sprite particleSprite in particleSprites)
            {
                if (target-- <= 0) break;

                GameObject particle = new GameObject("Particle");
                particle.transform.position = startPos;
                particle.transform.localScale = Vector3.one * 0.3f;

                SpriteRenderer sr = particle.AddComponent<SpriteRenderer>();
                sr.sprite = particleSprite;

                float angle = Random.Range(0f, 180f);
                float rad = angle * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

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
        SpriteRenderer sr = particle.GetComponent<SpriteRenderer>();
        Color startColor = sr.color;

        Vector2 velocity = direction;

        float gravity = -9.8f;

        Vector3 position = particle.position;

        while (elapsed < fadeDuration)
        {
            float deltaTime = Time.deltaTime;

            velocity.y += gravity * deltaTime;

            position += (Vector3)(velocity * deltaTime);
            particle.position = position;

            sr.color = Color.Lerp(startColor, Color.clear, elapsed / fadeDuration);

            elapsed += deltaTime;
            yield return null;
        }

        Destroy(particle.gameObject);
    }


    private IEnumerator FallAnimation(int delay, GridItem item, int dY)
    {
        item.gameObject.SetActive(true);

        if (dY <= 0) yield break;

        GameObject obj = item.gameObject;
        Vector3 start = obj.transform.position;
        Vector2Int startPos = GridPositionCalculator.Instance.GetGridPosition(start);
        Vector3 end = GridPositionCalculator.Instance.GetWorldPosition(startPos.x, startPos.y - dY);
        float unitDuration = 0.05f;
        float duration = unitDuration * dY;
        float delayt = 0;
        float delayDuration = unitDuration * delay;
        while (delayt < delayDuration)
        {
            delayt += Time.deltaTime;
            yield return null;
        }
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

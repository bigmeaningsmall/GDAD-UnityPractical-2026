using System.Collections.Generic;
using UnityEngine;

// Makes a set of bullets once and hands them out over and over.
public class BulletPool : MonoBehaviour
{
    public Bullet bulletPrefab;      // the Bullet prefab - note the type, not GameObject
    public int startingSize = 30;

    // A Queue is a list you add to one end of and take from the other.
    // Any switched-off bullet will do, so the order doesn't matter to us.
    private Queue<Bullet> available = new Queue<Bullet>();

    private void Awake()
    {
        // Make them all now, so nothing has to be created once the game is running
        for (int i = 0; i < startingSize; i++)
        {
            Bullet bullet = CreateBullet();

            available.Enqueue(bullet);
        }
    }

    // Makes one bullet, switched off and parented to this object
    private Bullet CreateBullet()
    {
        Bullet bullet = Instantiate(bulletPrefab, transform);

        bullet.gameObject.SetActive(false);

        return bullet;
    }

    public Bullet Get(Vector3 position, Quaternion rotation)
    {
        Bullet bullet;

        if (available.Count > 0)
        {
            bullet = available.Dequeue();
        }
        else
        {
            // The pool ran dry. Make one more - it joins the pool when it comes back.
            bullet = CreateBullet();

            Debug.Log("Bullet pool ran out, made an extra one.");
        }

        // Move it into place BEFORE switching it on, so it doesn't appear
        // for a frame wherever the last bullet finished.
        bullet.transform.position = position;
        bullet.transform.rotation = rotation;

        bullet.SetPool(this);
        bullet.gameObject.SetActive(true);

        return bullet;
    }

    public void Return(Bullet bullet)
    {
        // Already back in the pool, so ignore it. Without this check the same
        // bullet could end up in the queue twice and get handed out to two shots.
        if (!bullet.gameObject.activeSelf)
        {
            return;
        }

        bullet.gameObject.SetActive(false);

        available.Enqueue(bullet);
    }

    // Handy for debugging - how many are sitting waiting
    public int AvailableCount()
    {
        return available.Count;
    }
}
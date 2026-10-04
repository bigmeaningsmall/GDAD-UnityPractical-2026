---
Order: 3-B
---
# **Week 3 - Part 2 : Interfaces**

### Overview

#### **What we are building**:
- An **`IDamagable`** interface - a contract that says "this thing can be damaged".
- Three completely different classes that *implement* it in their own way: **`Crate`**, **`ExplodingCrate`** and **`Enemy`**.
- A one-block change to **`Bullet`** so it can damage _any_ of them without knowing what they are.

#### **Why we are doing this**:
- Last week inheritance let us treat different items as the same _kind_ of thing. Interfaces do something related but different - they let us treat unrelated things as having the same _ability_.
- A crate is not an enemy. They share no base class and shouldn't. But both can be shot.
- This is the first step in **decoupling** - the bullet doesn't need to know what it hit.

#### **Learning Objectives**:
- **Interfaces**: Define a contract with no implementation.
- **Implementing an interface**: `: IDamagable` and what the compiler then demands.
- **Properties in an interface**: why an interface can demand a property but never hold a variable.
- **Programming to an abstraction**: `GetComponent<IDamagable>()` instead of `GetComponent<Crate>()`  or `GetComponent<Enemy>()`.
- **SOLID in practice**: see the Open/Closed and Dependency Inversion principles. This is extra info that you don't need to learn but I'm citing it if you want to understand these ideas.

---
---

# **Part 2: Interfaces and `IDamagable`**

Carry on in **Scene-Week3_Code-Communication**. You should be able to shoot, and have Crate, ExplodingCrate and Enemy objects in the scene doing nothing.

---

### **Step 1: What is an Interface? (read before coding)**

*An **interface** is a list of methods a class promises to have.* No code, no fields, no logic - just the promise or also referred to as a contract.

```csharp
public interface IDamagable
{
    void TakeDamage(int damage);
}
```

That's the whole interface. It says: _anything using or implementing `IDamagable` must have a `TakeDamage` method that takes an int._ It can do whatever it wants within that method but it must have the method.

**Compare it to last week's inheritance**:

| **Comparing**    | **Inheritance (`Item`)**    | **Interface (`IDamagable`)** |
| ---------------- | ----------------------- | ------------------------ |
| **Relationship** | "IS A kind of"          | "CAN DO"                 |
| **Gives you**    | Shared code you inherit | A promise, no code       |
| **How many?**    | One base class only     | As many as you like      |
| **Our example**  | HealthPotion IS AN Item | A Crate CAN BE damaged   |

The key limitation inheritance has: **a class can only inherit from one base class**. Our `Enemy` might later need to be a `Character`, and a `Crate` is clearly not a Character, so we could never force them into one hierarchy to share `TakeDamage`. Interfaces have no such limit - a class can implement as many as it needs.

> [!note] **Note - the `I` prefix.**
> 
> Naming interfaces `IDamagable`, `IInteractable`, `IMoveable` is a C# convention, not a rule. It's so you can tell at a glance that `: IDamagable` is a promise and `: Item` is a parent class. 
> Try to stick to conventions - Its easy to read and share and keeps the project in order

---

### **Step 2: Create the `IDamagable` Interface**

1. **Create the Script**:
    - In _Assets / _CRE340 - Game Tutorial / Scripts / Interfaces_, create a C# script called `IDamagable.cs`.
    - Delete **everything** Unity puts in it - an interface is not a MonoBehaviour and doesn't need `using UnityEngine;`. Alternatively you can just make a plain c# class.

```csharp
// IDamagable.cs
// An interface is a CONTRACT. Any class that implements it MUST provide these members.
// There is no code here - it is a promise that the code exists somewhere.

public interface IDamagable
{
    // An interface CANNOT hold a variable - 'public int health = 10;' is not allowed here.
    // But it CAN demand a PROPERTY. This says: you must provide a way to READ your health.
    int Health { get; }

    void TakeDamage(int damage);   // reduce health / apply damage
    void ShowHitEffect();          // show some visual feedback for the hit
}
```

**Explanation**:
- Methods in an interface have **no body** - they end with a semicolon, like the `abstract` methods from Week 2.
- No access modifiers either. Everything in an interface is public by definition - that's the point, it's the public contract.
- **`int Health { get; }`** is a property, not a variable. The interface isn't storing a health value - it's enforcing that every implementer provides a way to read one. _Where_ that number actually exists in the class can be set in any way.
- `{ get; }` with **no `set`** is deliberate. Anything can **read** a target's health, but nothing can go in and change it. The only way to change health is to call `TakeDamage` - which is exactly the rule we want.

> [!note] **Note - Why can't an interface hold a variable?**
> 
> An interface is a promise, not a thing. Nothing is ever created _from_ an interface, so there is nowhere for a variable to exist. A class has memory for its fields; an interface has none.
> 
> This is a difference from the `abstract` class in Week 2, which **could** hold fields - it had `protected ItemData data;`. Abstract class = partly-built thing. Interface = contract only.

---
### **Step 3: Implement `IDamagable` on the `Crate`**

1. **Create `Crate.cs`** in a new **Props** folder inside Scripts.
2. **Attach it to the Crate prefab.** 

- ***You'll have to make the prefabs for these because I forgot to make them and export them 
- Create a box, make a material, you can try a rigidbody if you want to try that, then add the script and make it a prefab.
- You might have to make it a big box or lower the bullet spawn so it doesn't fly over. You can tune either to the desired feel

```csharp
using System.Collections;
using UnityEngine;

// Note the TWO things after the colon:
// MonoBehaviour  - the class we INHERIT from (we get all the Unity behaviour)
// IDamagable     - the interface we IMPLEMENT (we promise to have its members)
public class Crate : MonoBehaviour, IDamagable
{
    public int health = 10;

    // Required by IDamagable - exposes our health field for reading.
    // Note the casing: lowercase 'health' is the actual variable, uppercase 'Health' is the public way in.
    public int Health
    {
        get { return health; }
    }

    private Material mat;
    private Color originalColor;

    private void Start()
    {
        mat = GetComponent<Renderer>().material;
        originalColor = mat.color;
    }

    // Required by IDamagable - the compiler will not let us build without it
    public void TakeDamage(int damage)
    {
        health -= damage;
        
        //TODO - We will add an event here later

        if (health <= 0)
        {
            Destroy(gameObject); // crate breaks when health runs out
        }
    }

    // Also required by IDamagable
    public void ShowHitEffect()
    {
        StartCoroutine(FlashColour(Color.green)); // this crate flashes green
    }

    // Coroutine - same technique as the pulse effect in Week 2 Part 2
    private IEnumerator FlashColour(Color flashColour)
    {
        mat.color = flashColour;

        yield return new WaitForSeconds(0.1f); // wait without freezing the game

        mat.color = originalColor;
    }
}
```

**Explanation**:

- `MonoBehaviour, IDamagable` - inherit from one class, implement any number of interfaces. The base class always comes first.
- The **`Health` property** is the same syntax we used for `PlayerStats.Data` in Week 2. We keep the lowercase `health` field because Unity can only serialise fields - that's the one the Inspector shows. The uppercase `Health` property is what the rest of the game is allowed to see.
- `ShowHitEffect` starts a **coroutine** - set the colour, wait a tenth of a second, set it back. Exactly the pattern from the Week 2 pulse effect, so you already know this one.
- Try deleting `TakeDamage` and look at the Console. The compiler error tells you exactly what the contract demands. **That's an interface earning its keep** - the promise is enforced before you ever press Play.

> [!note] **Note - Why a coroutine and not `Invoke("ResetMaterial", 0.1f)`?**
> 
> `Invoke` works, and you'll see it in older Unity tutorials. The problem is it takes the method name as a **string** - so if you rename `ResetMaterial`, nothing flags or goes red in the editor, but it silently stops working at runtime.
> 
> A coroutine is checked by the compiler like any other code. Rename it and the error appears immediately. We want to make our errors tracible and noticeable.

> [!note] **Note - a small thing to notice if you hold the fire button.**
> 
> Hit the crate twice quickly and the second flash starts before the first has finished, so the colour resets early. Not a bug worth fixing here, but have a think about how you'd solve it - a bool guard like `canClick` in Week 2, or `StopAllCoroutines()` before starting the new one. Both would work.

---

### **Step 4: Implement `IDamagable` on the `ExplodingCrate`**

Same contract with a different behaviour.

1. **Create `ExplodingCrate.cs`** in the Props folder and attach it to the ExplodingCrate prefab.

```csharp
using System.Collections;
using UnityEngine;

public class ExplodingCrate : MonoBehaviour, IDamagable
{
    public int health = 10;
    public GameObject explosionEffectPrefab; // optional - drag a particle effect here

    // Same property, same job - this class stores its health exactly like the Crate does
    public int Health
    {
        get { return health; }
    }

    private Material mat;
    private Color originalColor;

    private void Start()
    {
        mat = GetComponent<Renderer>().material;
        originalColor = mat.color;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        
        //TODO - We will add an event here later

        if (health <= 0)
        {
            Explode();              // the thing that makes this crate different
            Destroy(gameObject);
        }
    }

    public void ShowHitEffect()
    {
        StartCoroutine(FlashColour(Color.red)); // this one flashes red - we could also make this an exposed variable and set it in the editor to any colour
    }

    private IEnumerator FlashColour(Color flashColour)
    {
        mat.color = flashColour;

        yield return new WaitForSeconds(0.1f);

        mat.color = originalColor;
    }

    private void Explode()
    {
        // Spawn an explosion effect if one has been assigned
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }
    }
}
```

---

### **Step 5: Implement `IDamagable` on the `Enemy`**

1. **Create `Enemy.cs`** in a new **Enemy** folder inside Scripts and attach it to the Enemy prefab.


```csharp
using System.Collections;
using UnityEngine;

//TODO - We may refactor this class eventually to use a base class and inheritance - for now its just a monobehaviour with an interface
public class Enemy : MonoBehaviour, IDamagable
{
    public int health = 10;

    public int Health
    {
        get { return health; }
    }

    private Material mat;
    private Color originalColor;

    private void Start()
    {
        mat = GetComponent<Renderer>().material;
        originalColor = mat.color;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        
        //TODO - We will add an event here later

        if (health <= 0)
        {
            Die();
        }
    }

    public void ShowHitEffect()
    {
        StartCoroutine(FlashColour(Color.red));
    }

    private IEnumerator FlashColour(Color flashColour)
    {
        mat.color = flashColour;

        yield return new WaitForSeconds(0.1f);

        mat.color = originalColor;
    }

    private void Die()
    {
        // TODO - later weeks: drop loot, award XP, play an animation
        Destroy(gameObject);
    }
}
```

> [!note] **Note - `Enemy` will be important to the rest of the module.**
> 
> It stays this simple for now, but from Week 7 we'll make a Factory to creates enemies, and in Week 8 make a State Pattern to give them behaviour. The `Die()` method with its TODO is where we can come back and add some extra functionality

---

### **Step 6: Make the Bullet Use the Interface**

This is the important part where we actually use the `Interface`

Open `Bullet.cs` and fill in the `TODO` from Part 1.

```csharp
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Stop the bullet dead and let it drop - change if you want
        rb.linearVelocity = Vector3.zero;
        rb.useGravity = true;

        // Ask the thing we hit: do you implement IDamagable?
        // TryGetComponent gives us a bool AND the component in one call
        if (collision.gameObject.TryGetComponent<IDamagable>(out IDamagable damageable))
        {
            // We have NO IDEA what we just hit. Crate? Enemy? Something we'll add in the future?
            // Doesn't matter - it promised it has these members, so we can use them.
            damageable.TakeDamage(damage);
            damageable.ShowHitEffect();

            // The Health property lets us ASK the target a question without knowing what it is 
            Debug.Log("Hit something - " + damageable.Health + " health remaining");
        }
    }
}
```

**Explanation**:

- `TryGetComponent<IDamagable>(out ...)` asks the object we hit whether it implements the interface, and hands it over if so. If it hit a wall, we skip the block and nothing happens.
- **The bullet never mentions `Crate`, `ExplodingCrate` or `Enemy`.** It only mentions `IDamagable`. 
	- That's decoupling. 
- `TakeDamage(damage)` on a crate destroys a crate. On an exploding crate it detonates. On an enemy it kills it. **One call but three behaviours** - the same polymorphism idea from Week 2
	- We can easily add things that implement the `IDamagable` interface, make them take damage in a specific way without having the damage dealer knowing what its hit.
- `damageable.Health` reads a value out of an object whose type we don't know. 
	- This is for an example that you can use an interface to ask a question or return a value without know what the value is because it came though the common interface. 

---

### **Step 7: Test It**

1. **Press Play** and shoot each object type.
    - Crate flashes green, breaks after 10 hits.
    - ExplodingCrate flashes red, spawns its effect when destroyed. - Placeholder particles for now.
    - Enemy flashes red and dies.
    - Shoot a wall - the bullet stops, nothing else happens, no errors.
2. **Test the concept**:
    - Make a new object. Give it a script with `: MonoBehaviour, IDamagable` and whatever `TakeDamage` behaviour you like - a barrel that launches into the air, a target that just logs a message.
    - Shoot it. **It works immediately, and you did not open `Bullet.cs`.**
        - The shooting is decoupled from the damaged objects but can do damage to lots of different types of object.
3. **Try breaking the contract**:
    - Comment the `Health` property from `Crate`. The project won't compile, and the error names the exact member you owe. Put it back.
    - Now try `damageable.Health = 0;` in `Bullet`. Also refused - there's no `set`. The only route to changing health is `TakeDamage`, which is the rule the interface enforces on everything.

---

### **SOLID - two principles you just used**

*There is a supporting lecture/slides on SOLID principles. You don't have to memorise or learn these as hard rules... but its useful as general guides on professional code practice.*

*I won't cover them as a full lecture or for the class test but we'll revisit them here and there in the module. You might cite one or two in your diagram or final project* 

In saying that.. we are using two SOLID principles here.

**O - Open/Closed Principle**: _software should be open to extension, closed to modification._ You just extended the game with a new damageable type without modifying `Bullet.cs`. 

**D - Dependency Inversion Principle**: _depend on abstractions, not concrete implementations._ `Bullet` depends on `IDamagable` (an abstraction), not on `Crate` (a concrete class). It's the difference between the code saying "I need a crate" or "I need something I can damage". 

The alternative approach to this is important to consider. 
- Without the interface, `Bullet.cs` is a chain of `if (GetComponent<Crate>() != null) ... else if (GetComponent<Enemy>() != null) ...` that gets longer every time anyone adds a destructible object, in a file that every weapon in the game depends on. 
- Everything you add becomes another conditional statement. If we had 50 things we'd have to add a new statement and change the shooting script every time. 


> [!note] **Note - What this does NOT solve.**
> 
> The bullet still has to _be there_ and _physically hit_ the thing. It calls `TakeDamage` directly on an object it's holding a reference to. That's fine for a bullet hitting a crate - they did collide.
> 
> But what about the **UI score counter** that wants to know a crate broke? The **audio system** that wants to play a sound? The **enemy spawner** that wants to know how many are left? None of them are anywhere near the collision, and we don't want the bullet holding references to all of them.
> 
> **Part 3** addresses that with **events**.

---

### **What we've set up**:

- An **`IDamagable`** interface - a contract with no implementation, demanding two methods and one readable property.
- Three unrelated classes implementing it, each in its own way.
- A **`Bullet`** that damages anything damageable without knowing what it is. 

### **The idea to take away**:

Inheritance asks _"what is this thing?"_. An interface asks _"what can this thing do?"_.

Use Interfaces in your projects. Grab the damage interface example from here and use it in your coursework. By using them you are decoupling and abstracting which has marks in the coursework.
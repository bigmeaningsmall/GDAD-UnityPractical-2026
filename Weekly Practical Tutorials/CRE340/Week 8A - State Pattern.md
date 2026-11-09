---
Order: 8-A
---
# **Week 8 : The State Pattern**

### Overview

#### **What we are building**:

- An **`IEnemyState`** interface, with **Idle**, **Chase** and **Patrol** states behind it.
- An `Enemy` that holds a current state and delegates its `Update` to it.
- A small **state label** above each enemy so you can watch the transitions.

#### **Why we are doing this**:

- Our enemies have one behaviour - straight at the player. Adding a second behaviour means `if` and `else` in `Update`, and a third means more.
- Week 7 was about getting variety in **stats** from the data assets. This week is variety in **behaviour**, driven from the same assets.
- Each state becomes a file you can read on its own, which is a lot easier than reading a long `Update`. The idea of a state pattern is to separate the states.

#### **Learning Objectives**:

- **The State Pattern** - one object with several behaviours that get swapped at runtime.
- **Context and state** - which class owns the data and which class owns the behaviour.
- **Reusing objects rather than creating them**

---

---

# **Enemy States**

Carry on from Week 7. Single tutorial again - one new interface, three small state classes, and some code moved out of `Enemy`.

---

### **Step 1: What the Enemy Does Now**

Open `Enemy.cs` and look at `Update`. It moves towards the player, every frame, from the moment it spawns.

That is the only thing our enemies can do. If we wanted them to wait until you came close, or wander about when you're not around, all of it would end up in `Update` behind conditions:

```csharp
    private void Update()
    {
        if (playerIsClose)
        {
            // chase
        }
        else if (shouldWander)
        {
            // patrol
        }
        else
        {
            // stand still
        }
    }
```

That works for two behaviours and gets complicated at five. Each new behaviour means editing a method that already does three other things, and the conditions start to interact.

**What we want:** each behaviour in its own class, and an `Enemy` that only has to know which one is currently active.

---

### **Step 2: Add a Chase Range to `EnemyData`**

An enemy needs to know how close the player has to get before it reacts. That's a per-type setting, so it goes in the data.

**In `EnemyData.cs`**, add:

```csharp
    [Header("Behaviour")]
    public float chaseRange = 8f;    // how close the player gets before this enemy chases
```

Set it on each of your data assets. Different values per type is the point - give **Bad Guy - Green** 6, **Bad Guy - Blue** 10, and **Bad Guy - Purple** 14 if you made it last week. The purple one will come after you from further away.

> [!note] **Note - why this isn't copied onto the Enemy**
> 
> Back in Week 4D we separated authored data from runtime state. `health` gets copied out of the data into the `Enemy` because it changes while the game runs - the asset holds the starting value, the enemy holds the current one.
> 
> `chaseRange` never changes, so there's no reason for a second copy of it. The states read `enemy.enemyData.chaseRange` directly. Two copies of a value that should always agree is the same problem we took out of the GameManager in Week 5.

---

### **Step 3: The `IEnemyState` Interface**

1. **Create a folder** called **EnemyState** inside _Scripts/Enemy_. All the state files go in there.
2. **Create a C# script called `IEnemyState.cs`** and delete the Unity boilerplate.

```csharp
// Every enemy state has the same three moments in its life, plus a name for the label.
public interface IEnemyState
{
    string Name { get; }        // shown on the state label above the enemy

    void Enter(Enemy enemy);    // called once, when this state starts
    void Update(Enemy enemy);   // called every frame while this state is active
    void Exit(Enemy enemy);     // called once, when this state ends
}
```

**Explanation**:

- Same idea as `IDamagable` - a contract. Anything claiming to be an enemy state has to provide these four members.
- **`Enter` and `Exit` run once each**, at the start and end. `Update` runs every frame in between. That's the lifecycle, and it's the same three moments whichever state you're writing.
- **The enemy is passed in as a parameter.** A state has no idea which enemy it's working on until it's told, which means one state class works for every enemy in the scene.
- `Name` returns a plain string rather than us reading the class name back off the type at runtime. Cheaper, and it means the label can say "Chase" while the class is called `EnemyState_Chase`.

> [!note] **Note - the standalone example used different names**
> 
> The standalone State Pattern example uses `IState` with `Enter` / `Tick` / `Exit`, and each state is handed its agent in a constructor.
> 
> Same pattern, different vocabulary. `Tick` is `Update`, `IState` is `IEnemyState`, and we pass the enemy as a parameter instead of storing it. The parameter version has less to set up, which is why the game uses it.

---

### **Step 4: Refactor the `Enemy`**

The `Enemy` stops doing the moving and now holds a state. In Pattern terms it becomes the **context** - it owns the data and the transitions, the states own the behaviour.

Each part below is the whole method after the change, so you can see it alongside the code that was already there.

**1. In `Enemy.cs`, add these fields.** They go after the `Health` property. **Delete `private Transform playerTransform;`** while you're there.

```csharp
    private Material mat;
    private Color originalColor;

    // ---------- State ----------

    // The state that is currently running
    private IEnemyState currentState;

    // Built once in Awake and reused, so changing state doesn't create anything
    public EnemyState_Idle IdleState { get; private set; }
    public EnemyState_Chase ChaseState { get; private set; }
    public EnemyState_Patrol PatrolState { get; private set; }

    // Fired when the state changes - the state label listens to this
    public event System.Action<string> OnStateChanged;
```

**2. `Awake` gets three lines at the top.** Everything already in there stays as it is.

```csharp
    private void Awake()
    {
        // The states need nothing from the data, so build them now and keep them.
        IdleState = new EnemyState_Idle();
        ChaseState = new EnemyState_Chase();
        PatrolState = new EnemyState_Patrol();

        // An enemy placed in the scene by hand already has its data, so set up now. means we can have enemies in the scene already or instantiate them
        // Enemies made by the factory arrive and get Initialise called instead.
        if (enemyData != null)
        {
            Initialise(enemyData);
        }
    }
```

**3. `Start` loses its player lookup** and sets the first state instead.

```csharp
    private void Start()
    {
        // Start runs after Awake and after the factory calls Initialise, so the data is ready by now.
        SetState(IdleState);

        // todo - call sound for enemy spawn
    }
```

The `FindFirstObjectByType<PlayerStats>()` search and the `playerTransform` field both go. `Update` never used `playerTransform` - it asked the GameManager instead - so the field was already doing nothing. Worth keeping it that way: a search done once at `Start` goes stale the moment the player dies and respawns, and the GameManager always has the current one.

**4. Replace `Update`** with this.

```csharp
    private void Update()
    {
        // The chase that used to be here has moved into EnemyState_Chase.
        if (currentState != null)
        {
            currentState.Update(this);
        }
    }
```

**5. Add `SetState` and the range check**, after `Update`.

```csharp
    // Swap to a new state, giving the old one a chance to tidy up first
    public void SetState(IEnemyState newState)
    {
        if (newState == null)
        {
            return;
        }

        if (currentState != null)
        {
            currentState.Exit(this);
        }

        currentState = newState;
        currentState.Enter(this);

        // Tell anything listening that we've changed
        if (OnStateChanged != null)
        {
            OnStateChanged(currentState.Name);
        }
    }

    // Idle and Patrol both need this, so it is written once here
    public bool IsPlayerInRange()
    {
        // Ask the GameManager where the player is.
        // There might not be one - not spawned yet, or dead - so check before using it.
        if (GameManager.Instance.Player == null)
        {
            return false;
        }

        Transform player = GameManager.Instance.Player.transform;

        float distance = Vector3.Distance(transform.position, player.position);

        return distance < enemyData.chaseRange;
    }
```

`Initialise`, `TakeDamage`, `ShowHitEffect`, `FlashColour`, `Die` and `OnCollisionStay` are all untouched.

**Explanation**:

- **`Update` is now one line.** It doesn't know or care what the current state does. Adding a tenth state wouldn't change this method.
- **`SetState` always runs `Exit` then `Enter`**, in that order. Writing that once means no state has to remember to clean up after itself on the way out.
- **The three states are created once in `Awake`** and handed out as properties. More on why in the note below.
- **`IsPlayerInRange` includes the null check**, so no state has to repeat it. The player prefab gets destroyed and respawned, so this comes up.
- `{ get; private set; }` is the short form of the properties we used in Week 3 - readable from anywhere, writable only inside `Enemy`.

> [!note] **Note - why the states are created once**
> 
> My previous version (last year) did this on every transition:
> 
> ```csharp
> enemy.SetState(new EnemyState_Chase());
> ```
> 
> That makes a new object each time. With twenty enemies flipping between Idle and Chase as you move around the arena, that's a steady stream of objects being created and then thrown away for the garbage collector to deal with.
> 
> Nothing in a state needs to be fresh, so there's no reason to keep making them. Build the three once in `Awake`, hand out the same instances forever, and the transition is free.
> 
> This is Week 10's programming optimisation. Most performance work in games is like this - something cheap being done lots of times, rather than one slow thing.

---

### **Step 5: The Idle and Chase States**

*There will be errors until we have added the state classes.*

1. **Create `EnemyState_Idle.cs`** in the EnemyState folder:

```csharp
using UnityEngine;

// Enemy stands still for a few seconds, then wanders off.
public class EnemyState_Idle : IEnemyState
{
    public string Name { get { return "Idle"; } }

    private float idleDuration = 2f;    // how long to wait before patrolling
    private float timeIdle;

    public void Enter(Enemy enemy)
    {
        timeIdle = 0f;
    }

    public void Update(Enemy enemy)
    {
        // Chase takes priority over anything else
        if (enemy.IsPlayerInRange())
        {
            enemy.SetState(enemy.ChaseState);
            return;
        }

        timeIdle = timeIdle + Time.deltaTime;

        if (timeIdle >= idleDuration)
        {
            enemy.SetState(enemy.PatrolState);
        }
    }

    public void Exit(Enemy enemy)
    {
    }
}
```

2. **Create `EnemyState_Chase.cs`**:

```csharp
using UnityEngine;

// Enemy moves straight at the player until the player gets clear.
public class EnemyState_Chase : IEnemyState
{
    public string Name { get { return "Chase"; } }

    public void Enter(Enemy enemy)
    {
    }

    public void Update(Enemy enemy)
    {
        // There might not be a player - not spawned yet, or dead - so check before using it
        if (GameManager.Instance.Player == null)
        {
            enemy.SetState(enemy.IdleState);
            return;
        }

        Transform player = GameManager.Instance.Player.transform;

        // This is the chase that used to be in Enemy.Update
        enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, player.position, enemy.enemyData.moveSpeed * Time.deltaTime);

        // Give up once the player is out of range
        if (Vector3.Distance(enemy.transform.position, player.position) > enemy.enemyData.chaseRange)
        {
            enemy.SetState(enemy.IdleState);
        }
    }

    public void Exit(Enemy enemy)
    {
    }
}
```

**Explanation**:

- **The chase code is a straight move from `Enemy.Update`.** `transform` became `enemy.transform` and `enemyData` became `enemy.enemyData`, and that's the whole change. The state has no `transform` of its own because it isn't a MonoBehaviour and isn't on any GameObject.
- **Each state decides when to leave itself.** Idle watches for the player and the timer, Chase watches for the player getting away. Neither one knows about the other's conditions.
- **`return` after `SetState`** so the rest of the method is skipped. Without it you'd carry on running the behaviour of a state you've just left.
- **`Enter` and `Exit` are empty in both.** They're part of the contract and they'll work in Step 8 and in the optional section - leaving them blank is fine.
- The states govern **movement only**. Contact damage is still in `OnCollisionStay` on the `Enemy`, and `TakeDamage` still works the same way whatever state the enemy is in. The pattern doesn't have to own everything the object does.

> [!note] **Note - the states know about each other**
> 
> `EnemyState_Idle` names `EnemyState_Chase`, and Chase names Idle. They're coupled, and if you add six more states the web of who-can-reach-who gets hard to follow.
> 
> The alternative is to take transitions off the states entirely and put them in a table or a separate state machine class that reads conditions and decides. That's how a large project with twenty states would do it. We're just trying to work an example of how to construct a State Pattern and the different ways it might take shape.
> 
> For three states it would be more machinery than the problem needs, and the transitions would be harder to find rather than easier - they'd be in a table somewhere instead of in the state you're reading. Worth knowing that the option exists and what the trade-off is.

---

### **Step 6: The State Label**

A label above each enemy showing its current state, so transitions are visible. This is just for debugging and showing the states in action

1. **On the `Enemy` prefab**, add a child object with a **TextMeshPro - Text** component (3D world text, not the UGUI one), positioned above the enemy. Shrink the font until it looks right.
2. **Create `EnemyState_UI.cs`** and put it on that child object:

```csharp
using UnityEngine;
using TMPro;

// Debug label showing which state the parent enemy is in.
public class EnemyState_UI : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh;

    private Enemy enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();

        if (textMesh == null)
        {
            textMesh = GetComponent<TextMeshPro>();
        }
    }

    private void OnEnable()
    {
        if (enemy != null)
        {
            enemy.OnStateChanged += ShowState;
        }
    }

    private void OnDisable()
    {
        if (enemy != null)
        {
            enemy.OnStateChanged -= ShowState;
        }
    }

    // Runs when the enemy changes state, not every frame
    private void ShowState(string stateName)
    {
        if (textMesh != null)
        {
            textMesh.text = stateName;
        }
    }
}
```

**Explanation**:

- **Subscribe in `OnEnable`, unsubscribe in `OnDisable`**. The enemy gets destroyed when it dies, so a subscription can be a memory leak.
- **The label updates when the state changes**, not every frame. This is the Week 6 Observer idea applied to something small - the display reacts to a change rather than asking repeatedly whether anything happened.
- `GetComponentInParent` finds the `Enemy` from the child, so the label doesn't need anything dragged into it.

---

### **Step 7: Set It Up and Test**

1. **Press Play.** You should see enemies standing still, then wandering, then turning on you when you get close.
2. **Watch the labels.** Walk towards an enemy and its label flips to Chase. Move away and it goes to Idle.
3. **Check the Console.** Nothing should be erroring. If you get a null reference on `enemyData`, the data assets aren't assigned on the spawner.

**This changes how the game plays.** Enemies used to move to you from the moment they spawned. Now they ignore you until you're inside their `chaseRange`, so now we can setup different spawn areas and have enemies feel like they react to us rather then just move. If the area feels too quiet, raise the ranges on the data assets.

> [!note] **Note - if a label flickers between Idle and Chase**
> 
> An enemy sitting right on the edge of its range can flip states every frame. Both the entering and leaving checks use the same distance, so a tiny bit of movement crosses it repeatedly.
> 
> The usual fix is to give up at a slightly larger distance than you start at - chase from 8, give up at 10. Then the enemy has to properly get away before it loses interest.

---

### **Step 8: Patrol**

**The point to using the State pattern here**. A new behaviour is one new file and two lines elsewhere. Each behaviour we add is its own file.

1. **Create `EnemyState_Patrol.cs`**:

```csharp
using UnityEngine;

// Enemy walks to a random point nearby, then stands still again.
public class EnemyState_Patrol : IEnemyState
{
    public string Name { get { return "Patrol"; } }

    private float patrolRange = 5f;       // how far it will wander from where it starts
    private float patrolSpeed = 1.5f;     // slower than the chase
    private float arriveDistance = 0.2f;  // close enough to count as arrived

    private Vector3 patrolTarget;

    public void Enter(Enemy enemy)
    {
        // Pick a point near wherever the enemy happens to be standing
        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);

        patrolTarget = enemy.transform.position + new Vector3(randomX, 0f, randomZ);
    }

    public void Update(Enemy enemy)
    {
        // Chase still takes priority
        if (enemy.IsPlayerInRange())
        {
            enemy.SetState(enemy.ChaseState);
            return;
        }

        enemy.transform.position = Vector3.MoveTowards(
            enemy.transform.position,
            patrolTarget,
            patrolSpeed * Time.deltaTime
        );

        // Arrived, so stand about for a bit before picking another point
        if (Vector3.Distance(enemy.transform.position, patrolTarget) < arriveDistance)
        {
            enemy.SetState(enemy.IdleState);
        }
    }

    public void Exit(Enemy enemy)
    {
    }
}
```

**That's it.** `Enemy` already has the `PatrolState` property from Step 4 and `EnemyState_Idle` already sends enemies here. Nothing else changes - not `Enemy.Update`, not the label, not the spawner, not the factory.

**Explanation**:

- **`Enter` picks the destination**, so each visit to Patrol goes somewhere new. This is what `Enter` is for - setup that happens once per visit rather than every frame.
- **Idle and Patrol pass back and forth.** Walk to a point, pause for two seconds, walk somewhere else. It reads as an enemy milling about rather than one freezing in place.
- **The patrol target is relative to where the enemy is standing**, not to a fixed home position, so over time they drift. If you want them held inside the arena, clamp `patrolTarget` to your spawn area.

---

### **Optional - An extra chase hook for CRE343**

`Enter` and `Exit` on Chase are still empty. If you publish on them, anything can react to an enemy getting aggressive without knowing anything about enemies - which is what the audio and VFX work needs.

**Add to your bus messages `GameMessages` file**:

```csharp
public struct EnemyChaseStarted
{
    public Transform enemy;
}

public struct EnemyChaseEnded
{
    public Transform enemy;
}
```

**In `EnemyState_Chase`**:

```csharp
    public void Enter(Enemy enemy)
    {
        EventBus<EnemyChaseStarted>.Publish(new EnemyChaseStarted { enemy = enemy.transform });
    }

    public void Exit(Enemy enemy)
    {
        EventBus<EnemyChaseEnded>.Publish(new EnemyChaseEnded { enemy = enemy.transform });
    }
```

Check the publish call against your own `EventBus` from Week 6 - use whatever you named the method there.

A glow that comes on when an enemy starts hunting you and fades when it loses you costs two subscriptions and no change to the enemy. Same arrangement as the `EnemyKilled` effects.

---

### **Where this is going**

**Week 9 - serialisation.** Saving the game means saving what each enemy was doing, which means a state needs something you can write down - an enum or a name - rather than only an object reference. _We might not save enemies but we'll do some basic saving and loading with the player_

**Week 10 - pooling.** A pooled enemy is reused rather than created, so it arrives still holding whatever state it died in. Resetting it is one line in `Initialise`, and it's the kind of bug that only shows up once objects start coming back from the dead. _We might do this depending on time and scope._

---

### **What we've set up**:

- An **`IEnemyState`** interface with **Idle**, **Chase** and **Patrol** behind it.
- An `Enemy` whose `Update` is one line and which owns transitions rather than behaviour.
- States **created once and reused**, so transitions allocate nothing.
- A **state label** driven by the change rather than polled every frame.
- Behaviour that varies per enemy type, from the same data assets Week 7 set up.
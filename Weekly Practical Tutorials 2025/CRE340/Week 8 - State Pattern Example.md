# **Guide: Implementing the State Pattern for Agent Behaviour in Unity**

This example demonstrates how to use the **State Pattern** to structure game logic in a clear and extensible way.  
It shows how to separate different behaviours (Idle, Move, Patrol) into their own classes so that your main agent script stays simple and easy to manage.

---

## **1. Concept Overview**

The **State Pattern** is used when an object (like an enemy or agent) can be in different **modes** or **states**, each with its own behaviour.

Instead of writing one large script with lots of `if` or `switch` statements, we separate each behaviour into its **own class**.  
This gives us a flexible and organised structure where each state handles its own logic, visuals, and updates.

When the agent changes state (for example, from Idle to Move), the **current state object is replaced**, and that new class takes over the behaviour.

This helps because:

- Each state is **self-contained**.
    
- You can easily **add or remove behaviours** without touching other code.
    
- It keeps the **main agent class simple** and focused on managing state transitions.
    

---

## **2. State Interface**

All states share a common structure, defined by an interface.  
The interface defines three methods that every state must implement:

```csharp
public interface IState
{
    void Enter(); // Called when the state starts
    void Tick();  // Called every frame
    void Exit();  // Called when the state ends
}
```

This guarantees that every state has consistent lifecycle methods.

---

## **3. The Agent (Context Class)**

The `MyAgent` script acts as the **context** that holds the current state.  
It doesn’t contain the logic for each behaviour — it simply updates whatever state is active and provides shared data (like speed and colour).

```csharp
using UnityEngine;

public class MyAgent : MonoBehaviour
{
    [Header("Visuals")]
    public Renderer rend;

    [Header("Movement")]
    public float moveSpeed = 1f;
    public float turnSpeed = 4f;
    public float smoothTime = 1.25f;

    [Header("Patrol")]
    public Vector3 patrolAOffset = new Vector3(-5, 0, 0);
    public Vector3 patrolBOffset = new Vector3(5, 0, 0);
    public float patrolSpeed = 1f;

    private IState currentState;

    private void Start()
    {
        if (rend == null)
            rend = GetComponent<Renderer>();

        ChangeState(new State_Idle(this));
    }

    private void Update()
    {
        currentState?.Tick();
    }

    public void ChangeState(IState next)
    {
        if (next == null) return;

        currentState?.Exit();
        currentState = next;
        Debug.Log($"[MyAgent] → {currentState.GetType().Name}");
        currentState.Enter();
    }

    public void SetColour(Color colour)
    {
        if (rend != null)
            rend.material.color = colour;
    }
}
```

**Key responsibilities:**

- Keeps a reference to the current `IState`.
    
- Calls `Tick()` each frame on that state.
    
- Handles transitions with `ChangeState()`.
    
- Provides shared data (e.g., colour, speed) that states can access.
    

---

## **4. State Controller**

The controller script allows you to manually switch between states using keyboard input.  
This is useful for testing and demonstration.

```csharp
using UnityEngine;

public class MyAgent_StateController : MonoBehaviour
{
    public MyAgent agent;
    public Transform moveTarget;

    private void Update()
    {
        if (agent == null) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            agent.ChangeState(new State_Idle(agent));
            Debug.Log("[Controller] 1 → Idle");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            agent.ChangeState(new State_MoveToTarget(agent, moveTarget));
            Debug.Log("[Controller] 2 → MoveToTarget");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            agent.ChangeState(new State_Patrol(agent));
            Debug.Log("[Controller] 3 → Patrol");
        }
    }
}
```

This script isn’t part of the pattern itself — it’s simply a way to **test and demonstrate** changing between different behaviours.

---

## **5. The State Classes**

Each state class implements the `IState` interface.  
They contain the logic for that specific behaviour and can access data from the agent if needed.

### **Idle State**

A simple default state. The agent stays still and can be extended later with a random wander.

```csharp
using UnityEngine;

public class State_Idle : IState
{
    private readonly MyAgent agent;

    public State_Idle(MyAgent agent) { this.agent = agent; }

    public void Enter()
    {
        agent.SetColour(Color.white);
        Debug.Log("[State_Idle] Enter");
    }

    public void Tick()
    {
        // Currently idle (can add simple wandering here)
        Debug.Log("[State_Idle] Tick");
    }

    public void Exit()
    {
        Debug.Log("[State_Idle] Exit");
    }
}
```

---

### **Move To Target State**

Moves smoothly towards a specific target in the scene, with rotation and colour change.

```csharp
using UnityEngine;

public class State_MoveToTarget : IState
{
    private readonly MyAgent agent;
    private readonly Transform target;
    private Vector3 velocity;

    public State_MoveToTarget(MyAgent agent, Transform target)
    {
        this.agent = agent;
        this.target = target;
    }

    public void Enter()
    {
        agent.SetColour(Color.green);
        velocity = Vector3.zero;
        Debug.Log("[State_MoveToTarget] Enter");
    }

    public void Tick()
    {
        if (target == null) return;
        MoveToTarget();
    }

    public void Exit()
    {
        Debug.Log("[State_MoveToTarget] Exit");
    }

    private void MoveToTarget()
    {
        agent.transform.position = Vector3.SmoothDamp(
            agent.transform.position,
            target.position,
            ref velocity,
            agent.smoothTime,
            Mathf.Infinity,
            Time.deltaTime
        );

        Vector3 dir = target.position - agent.transform.position;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, targetRot, agent.turnSpeed * Time.deltaTime);
        }
    }
}
```

---

### **Patrol State**

Moves back and forth between two points using smooth interpolation.

```csharp
using UnityEngine;

public class State_Patrol : IState
{
    private readonly MyAgent agent;
    private Vector3 start;
    private Vector3 velocity;

    public State_Patrol(MyAgent agent) { this.agent = agent; }

    public void Enter()
    {
        start = agent.transform.position;
        agent.SetColour(Color.cyan);
        Debug.Log("[State_Patrol] Enter");
    }

    public void Tick()
    {
        Patrol();
    }

    public void Exit()
    {
        Debug.Log("[State_Patrol] Exit");
    }

    private void Patrol()
    {
        Vector3 a = start + agent.patrolAOffset;
        Vector3 b = start + agent.patrolBOffset;
        float t = Mathf.PingPong(Time.time * agent.patrolSpeed, 1f);
        Vector3 target = Vector3.Lerp(a, b, t);

        agent.transform.position = Vector3.SmoothDamp(
            agent.transform.position,
            target,
            ref velocity,
            agent.smoothTime,
            Mathf.Infinity,
            Time.deltaTime
        );

        Vector3 dir = target - agent.transform.position;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion toRot = Quaternion.LookRotation(dir);
            agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, toRot, agent.turnSpeed * Time.deltaTime);
        }
    }
}
```

---

## **6. Why This Structure Helps**

1. **Encapsulation**  
    Each state controls its own logic and visuals, with no interference from other states.
    
2. **Extensibility**  
    Adding a new behaviour is as simple as creating a new class that implements `IState`.  
    Example: `State_Attack`, `State_Flee`, or `State_Search`.
    
3. **Simplified Debugging**  
    Each state can be tested and debugged in isolation with clear console logs.
    
4. **Readability**  
    The agent stays minimal. it doesn’t contain large blocks of conditional logic or a giant class for all the things the agent can do.
    
5. **Real-world Application**  
    Commonly used for AI characters, player controllers, game systems, or menus where behaviour changes over time.
    

---

## **7. How to Extend It**

Once the pattern is in place, extending it is straightforward:

- Create a new class that implements `IState`.
    
- Define what happens in `Enter()`, `Tick()`, and `Exit()`.P
    
- Trigger it with `agent.ChangeState(new YourNewState(agent));`
    

For example, you could add:

- A **State_Wander** that makes the idle state drift randomly.
    
- A **State_Attack** that moves toward and damages a player.
    
- A **State_Dead** that plays an animation and disables movement.
    

---

## **8. Key Takeaways**

- The **State Pattern** helps separate logic cleanly between behaviours.
    
- It reduces complexity and makes your code modular.
    
- It’s ideal for AI or systems where behaviours change dynamically.
    
- You can easily plug in new states without rewriting existing code.
    

---

**Summary:**  
By structuring behaviour this way, your agent remains simple and flexible.  
Each new feature or state lives in its own class, keeping the project easy to maintain and scale as your game grows.
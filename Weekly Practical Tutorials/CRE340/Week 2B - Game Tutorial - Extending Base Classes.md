---
Order: 2-B
---
# **Week 2 - Part 2 : Extending Base Classes**

### Overview

#### **What we are building**:

- The hierarchy already works - now we can go back into the **base class** and extend it.
- Every change we make in `Item` changes **every item type in the game** without touching a single derived class.
- We'll add spinning, click feedback, and per-type colours, then finish by promoting the base class to **`abstract`**.

#### **Why this matters**:

- This is the  payoff of inheritance and the bit that's hard to appreciate until you see it or have a game with lots of variation. **One edit, updates everything**
- The trade-off is a change to a base class needs to be careful not to break everything below it.

#### **Learning Objectives**:

- **Extending a base class**: Add shared behaviour in one place and have it apply across a hierarchy.
- **Calling base methods**: Use `base.Awake()` so derived classes keep inherited setup.
- **Coroutines**: Run a simple timed effect over several frames.
- **Abstract classes**: Stop a base class being used directly when it only exists to be inherited from.


---
---

# **Part 2: Extending the Base Class**

Carry straight on in the same scene - **Scene-Week2_Items-and-Classes**. You should have `PlayerData`, `PlayerStats`, `ItemData`, `Item`, `HealthPotion`, `ManaPotion`, and a spawner filling the scene.

So far our items demonstrate the OOP concepts, but they're static grey shapes and everything happens in the Console. 

The important thing to watch: **we only edit `Item.cs`**. `HealthPotion` and `ManaPotion` stay untouched, and both get everything. This is good structured programming work. We've already outlined the structure and now we are just extending the functionality across classes using a consistent design.

---

### **Step 1: Spinning Items (shared behaviour in the base class)**

Spinning collectable is a common simple code animation. We can do this in the base class.

1. **Add to the `Item` class**:

```csharp
    [Header("Item Movement")]
    [SerializeField] protected float rotationSpeed = 100f; // degrees per second

	// Every item rotates - defined ONCE here, inherited by every item type - notice the protected keyword so derived classes can still reach it, but unrelated scripts can't.  
	protected virtual void Update()  
	{  
	    // Rotate slowly around the Y-axis   
		transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime); // not the most efficient way to rotate or use Update(), but its simple and works for this example  
	}
```

2. **Press Play**.
    - Every health potion AND every mana potion is now spinning.
    - We didn't open `HealthPotion.cs` or `ManaPotion.cs`. We just add to the base class.

**Explanation**:

- The field is **`protected`** so derived classes can reach it, and **`[SerializeField]`** so it still shows in the Inspector on each prefab. You can give mana potions a different spin speed without any new code.
- `Update()` is marked **`virtual`**, so a derived class _could_ override it if one item needed to move differently.
- It seems like a lot of extra labels we need to remember but once you start using them its very standard and logical.

> [!note]
> ***Note - Careful here. If a derived class ever writes its own `Update()` without the `override` keyword, it HIDES the base version instead of replacing it and the spinning quietly stops.** 
> 
> Unity should warn you in the Console. If you override it and still want the spin, call `base.Update();` inside your version - exactly like our potions already do with `base.Awake()`.



---

### **Step 2: Pulse Effect on Click (interaction feedback in the base class)**

Let's give some visual feedback when an item is clicked, using a **coroutine**.

1. **Add to the `Item` class** (and make sure `using System.Collections;` is at the top of the file): 
	- This is good practice to `Import/Add/Use libraries` as you need them 

```csharp

// ---------- More SPECIALISED BEHAVIOUR (Click example - OK to put the bool here as its just an example - normally I would keep the variables at the top of the class) ----------

    // To prevent multiple clicks during the pulse effect
    private bool canClick = true;

    // OnMouseDown is a built-in Unity message - it fires when this object's collider is clicked
    protected virtual void OnMouseDown()
    {
        if (canClick)
        {
            canClick = false;
            StartCoroutine(PulseEffect());
        }
    }

    // Coroutine to handle the pulse effect - runs across several frames
    private IEnumerator PulseEffect()
    {
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 1.2f;    // scale up
        
        yield return new WaitForSeconds(0.2f);          // wait, without freezing the game
        
        transform.localScale = originalScale;           // scale back down
        canClick = true;                                // re-enable clicking
    }
    
```

2. **Press Play** and click any item - it pops briefly.

**Explanation**:

- A **coroutine** is a method that can pause and resume. `yield return new WaitForSeconds(0.2f)` waits without blocking everything else.
- `canClick` stops a fast clicker from stacking the effect on top of itself.
- Every item type gets this. Again, one edit.

** **Note - `OnMouseDown` needs a collider, which our items have. It's a quick built-in for prototyping, so it's a demo tool but a good example if you want to use click interactions.**

---

### **Step 3: Visual Differentiation by Subclass (fancy name for... custom colours!)**

All our items look the same. We'll give each type its own colour - the mechanism is defined once in the base class, the colour chosen per type.

This is where the empty `Awake()` we left in the base class is used

1. **Add this field to the `Item` class**:

```csharp
    [Header("Item Appearance")]
    [SerializeField] protected Color itemColor = Color.white; // set per-prefab in the Inspector
```

2. **Fill in the base class `Awake()`** - it's currently empty, so replace it with this:

```csharp
    // Shared setup that EVERY item gets, whatever type it is
    protected virtual void Awake()
    {
        Renderer itemRenderer = GetComponent<Renderer>(); // get the renderer component 

        if (itemRenderer != null)
        {
            itemRenderer.material.color = itemColor; // access the material and set the colour
        }
    }
```

3. **Set the colours on the prefabs**:
    - Open the **HealthPotion** prefab and set **Item Color** to red.
    - Open the **ManaPotion** prefab and set **Item Color** to blue.
4. **Press Play** - red health potions, blue mana potions, from one shared code path.
    

**Explanation**:
- The **mechanism** lives in the base class (read a colour, apply it to the renderer). The **value** is set per type.
- Shared behaviour with different results. Same idea as `Use()`, just applied to how things look rather than what they do.

> [!Base note]
> **Note - THIS is why we wrote `base.Awake();` at the top of the potions' Awake methods in Part 1.** 
> 
> We just added functionality to the base class, and because both derived classes already call up to it, they picked it up automatically. 
> 
> Delete that one line from HealthPotion and its colour stops working - everything else keeps running, no errors. Try it, then put it back. Forgetting `base.` is one of the most common inheritance bugs thats hard to see.
> 

> [!Instance note]
> **Note - Setting `.material.color` at runtime creates a new material instance for each object.** 
> 
> Fine at this scale, but it's a performance cost that adds up. This is something we'll look at in optimisation when we consider how many instanced objects we want or where they can be shared


---

### **Step 4: Promote `Item` to an `abstract` Class**

One last change, and this one's a design decision rather than a feature.

**Note:** *This is an advanced concept but worth implementing as a good example of how to use abstracts* 
- If you skip this step it 'shouldn't' break anything in later weeks (I think). This is just to prevent the possibility of Items without types being created. Think about it in the context of a huge game with 1000's of items when safety and defensive programming is required.

***Question?*** : **Have we ever created a plain `Item`?** 
- ***No. We only ever make health potions and mana potions.*** 

A bare `Item` isn't a 'thing' in the game - it's a template. And if one did end up in the scene by accident (games have bugs), its `Use()` would log "it did nothing" and the player would get nothing.

C# lets us say that explicitly with **`abstract`**.

1. **Change the class declaration in `Item.cs`**:

```csharp
// 'abstract' means: this class can be inherited from, but NEVER attached to a GameObject directly
public abstract class Item : MonoBehaviour
```

2. **Change the `Use` method from `virtual` to `abstract`**:

```csharp
    // 'abstract' method - no body at all. Every derived class MUST provide its own Use().
    public abstract void Use(PlayerStats player);
```

- Delete the old `virtual` version with the `Debug.Log` body. An abstract method has no body - it ends with a semicolon.

3. **Press Play** - everything works exactly as before.

**Explanation**:

- **`abstract class`**: the compiler now refuses to let anyone attach a bare `Item` to a GameObject. Try it out - the component won't be offered.
- **`abstract` method**: there is no default `Use()` any more. Every derived class is _forced_ to write one. Forget it, and you get a compile error instead of a silent do-nothing item at runtime.
- This turns a potential bug into something the compiler catches before you press Play.

> [!Abstract note]
> **Note - Order matters here**
> 
> *We started with a concrete* `virtual` *base so inheritance and overriding could be created and verified*, then improved once we understood that a plain Item is never used. 
> 
> Abstraction is something you gain from experience as you develop a code base. 
> 
> We'll repeat this again in Week 6 (`abstract InventoryItem`) and Week 7 (`abstract EnemyBase`) - so we'll get a better idea of its usage
> 
> Small bit of advice - Abstracts are advanced and not always required (in this context/scale) - Nice to have kind of thing


---

### **Summary - OOP, Classes and Inheritance**

Across both parts we've covered:

- **Classes and instances** - plain C# classes (`PlayerData`, `ItemData`) created with `new`.
- **Constructors** - default and parameterised, including overloading.
- **Inheritance** - shared fields and methods defined once in `Item`.
- **Polymorphism** - `Use()` called on the base type, the derived version running.
- **Method Overriding** - `virtual` / `override`, and calling `base.` to keep inherited behaviour.
- **Protected access** - base class data reachable by derived classes but not by the whole project.
- **Abstract classes** - a base class that exists only to be inherited from.
- **Composition** - behaviour layered onto GameObjects as separate components.

The big takeaway from Part 2: **every change was generally in one file and applied to every item in the game (so far).** That's the functionality a good base class has. It's also the risk with a wee bit of extra overhead to setup and potential to break everything if we don't follow a good structure. 

### **Next Steps**:

- The **`Item` hierarchy** is not a throwaway example. 
	- We can revisit and grow it into a full **inventory system** with more item types, and in **Week 9** we can use it to show **save and load** that inventory to a file. 
	- Once we learn the setup and pattern we can use it again and again. 
	- When we make a change we already know the structure and every change follows a pattern too.
- **`PlayerData`** and **`ItemData`** are already the right shape for that save system - that's why they're plain `[System.Serializable]` classes rather than MonoBehaviours. We'll extent it when we make the save but you'll see the pattern.
- We'll do some work in event driven UI in Week 5. The **`PlayerStats`** component can be wired to the UI without any direct call. 
	- Thats the best part of programming - Once you have a structure and event communication you can de-couple your code and make it super functional and free from bugs (thats the general idea).

---

#### **Note**: Remember to look up the difference between **Inheritance** and **composition**. Unity and most engines allow for different approaches and solutions to problems. 

##### The way I am doing it here is formal approach to programming. 

##### You might favour, composition, scriptable objects and prefabs as a workflow and achieve the same result. This is also viable but more editor driven whereas the inheritance/abstraction approach gives you a more robust code model. There is a certain amount of mixing and matching as you prototype and start to understand your own structure, game and workflow. Its iteration and returning to improve systems as you go.

---
### Last thing:  This was a lot and longer than I expected! 
##### Object Orientated Programming (OOP) is a big subject but this is a core structure that's pretty advanced but very repeatable. We'll come back and repeat the concepts in other contexts. You don't have to know this by memory. Just have a understanding of the concept of inheritance between classes.

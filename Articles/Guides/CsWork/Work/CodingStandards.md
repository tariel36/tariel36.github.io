# General
* Avoid anonymous objects until it's justified;
* Use [ValueTuple](https://docs.microsoft.com/en-us/dotnet/api/system.valuetuple?view=net-6.0) with proper names instead of `Tuple` class;
* Don't use static usings;
* Don't use type aliases;
* All blocks must have bracers;
* Prioritize overrides over new keyword;
* Prioritize `using` when dealing with disposables;
* Prioritize auto properties over implemented properties;
* Prioritize `System.HashCode.Combine` over manual calculation;
* Use bracers for clarity when dealing with expressions;
* Use object initializers when possible;
* Use collection initializers when possible;
* Always use simplified boolean expressions;
* Prioritize new switch expressions;
* Prioritize conditional operators instead of if else with assignments;
* Prioritize conditional operators with returns;
* Prefer simple `default` expressions;
* Never use expression body except for lambdas;
* Prefer pattern matching when possible;
* Prefer inlined declaration;
* Prefer throw expressions;
* Don't use conditional delegate call;

# Naming
Various rules related to naming. In general, all names should provide meaningful names, so avoid abbreviations if possible. However it's not a hard rule, so if context is meaningful enough, it's fine to use shortcuts or single letter variables. For example, simple LINQ expressions are fine with arguments named like `x` or so.

## Complex types
* For all complex type declaration use [PascalCase](https://pl.wikipedia.org/wiki/PascalCase);
* All interface names should start with `I` letter, for example: `IMyInterface` - this is the only exception from [PascalCase](https://pl.wikipedia.org/wiki/PascalCase) rule;

## Members
* Non private fields should use [PascalCase](https://pl.wikipedia.org/wiki/PascalCase);
* Private (`private ...`) fields should use [CamelCase](https://en.wikipedia.org/wiki/Camel_case) convention and be prefixed with `_`, for example:
    * `private int _value;`;
    * `private static int _value;`;
* All properties should always use [PascalCase](https://pl.wikipedia.org/wiki/PascalCase);
* All events should use [PascalCase](https://pl.wikipedia.org/wiki/PascalCase);
* All methods should use [PascalCase](https://pl.wikipedia.org/wiki/PascalCase);
* Const members should always use [PascalCase](https://pl.wikipedia.org/wiki/PascalCase);

## Locals
* Local functions should use [PascalCase](https://pl.wikipedia.org/wiki/PascalCase);
* Local const should use [PascalCase](https://pl.wikipedia.org/wiki/PascalCase);

## Generic arguments
* Generic arguments should always provide meaningful name with `T` suffix, for example: `TKey`, `TValue`;

## Variables
* All variables should use [CamelCase](https://en.wikipedia.org/wiki/Camel_case) convention;

## Arguments
* Arguments should use [CamelCase](https://en.wikipedia.org/wiki/Camel_case) convention;

## Delegates
* Delegates should use [PascalCase](https://pl.wikipedia.org/wiki/PascalCase);

# Comments
* Only `//` should be used withing code;
* Only `///` should be used for documentation purposes;
* `/** */` Comments should be avoided;

# Variables
Various rules related to variables.

## `var` keyword
* Whenever any variable is declare, full type name should be used;
* The `var` keyword must be avoided until it's required by syntax (like anonymous objects);
* You can `use` var when used type is exceptionally complex, for example: `Dictionary<string, Dictionary<string, Dictionary<MyKeySpecifier<SomeType, List<Tuple<string, string, string>>>>>>>`

# Usings
Usings should always be placed outside namespace block and use following order:

```
System
External libs
Local libs
Current project
```

Example:

```CS
using System;                 // System;
using Nuke.Common;            // External library;
using Distribution.Platforms; // Local library;
using Pipes;                  // Namespace from current project;
```

# Complex types
All rules related to complex types.

## General
* `this` should be avoided until it's necessary to use it;
* Members of complex types and complex type itself must always have access modifier specified;

## Structure
How all the class member should be sorted.

### By type
* Delegates;
* Const;
* Static readonly;
* Static fields;
* Instance fields;
* Static constructors;
* Constructors;
* Static destructors;
* Destructors;
* Static events;
* Instance events;
* Static properties;
* Instance properties;
* Static methods;
* Instance methods;

### By access
* public;
* internal;
* protected internal;
* protected;
* private protected;
* private;

### `Optional` Properties
Properties are the specific case because of their possible implementations and access modifiers.

Recommended order by type:
* Auto properties, `get` only;
* Auto properties, `get` and `set`;
* Implemented properties `get` only;
* Implemented properties, `get` and `set`;

Recommended order by `get` and `set` access:
* public, public;
* public, private;

# File layout
* Indentation should always follow 4 spaces;
* Avoid multiple empty lines;
* Early method escapes should be single lines;
* Prioritize early method escapes instead of nesting;

## Complex types
Complex types should use following layout:

```XML
<usings />
<namespace>
    <complex-type />
</namespace>
```

Example:

```CS
using System;
using Nuke.Common;
using Distribution.Platforms;
using Pipes;

namespace MyNamespace
{
    public class MyType
    {

    }
}
```

## Delegates only
Files that contain only delegates should use following layout:
```XML
<usings />
<namespace>
    <delegates />
</namespace>
```

Example:

```CS
using System;
using Nuke.Common;
using Distribution.Platforms;
using Pipes;

namespace MyNamespace
{
    public delegate void SomeFunc(string arg);
}
```

## Files with multiple declarations
Files with multiple declarations should be avoided when possible (excluding the nested types). Otherwise, the files with multiple declarations should follow this order:

All complex types should be sorted and ordered by:
* public;
* internal;
* protected internal;
* protected;
* private protected;
* private;

And then each group should follow order:
* Delegates;
* Interfaces;
* Records;
* Structs;
* Classes;

For example:
```XML
<usings />
<namespace>
    <public>
        <delegate />
        <interface />
        <record />
        <struct />
        <class />
    </public>
    <private>
        <delegate />
        <interface />
        <record />
        <struct />
        <class />
    </private>
</namespace>
```

## Fields
When possible fields should be one-liners. However if initialization is complex, the default wrapping/chaining rules apply.

```CS
public int Test = 0;

public int LongTest = Enumerable.Range(0, 100)
    .Where(x => x > 50)
    .Select(x => x % 2 == 0 ? 10 : -10)
    .Sum();

```

## Properties
Properties should use following layouts:
```CS
public int Property { get; }

public int Property { get; set; }

public int Property { get { return x; } }

public int Property
{
    get { return x; }
    set { x = value; }
}

public int Property
{
    get { return x; }
    set
    {
        if (HasChanged(value, x))
        {
            x = value;
        }
    }
}

public int Property
{
    get
    {
        if (x > 100) { return 100; }

        return x;
    }
    set
    {
        if (HasChanged(value, x))
        {
            x = value;
        }
    }
}
```

## Constructors
* Constructors follow default functions/methods rules;
* Constructors follow default sorting rules;

## Functions & methods
By default, functions should be structured like this:

```CS
public int Add(int left, int right)
{
    return left + right;
}
```

When function contains multiple parameters or the declaration is really long, then it should be structured like that:
```CS
public int Add(
    int x1,
    int x2,
    int x3)
{
    return x1 + x2 + x3;
}
```

## Ternary operator

Inline version for short expressions:

```CS
string x = y % 2 == 0 ? "Yes" : "No";
```

Multiline version for long expressions (default):

```CS
string x = y % 2 == 0
    ? "Yes"
    : "No"
    ;
```

## Null coalescing operator

Inline version for short expressions (default):

```CS
string x = left ?? right;
```

Multiline version for long expressions:

```CS
string x = left
    ?? right
    ;
```

## Optional chaining operator

Inline version for short expressions (default):

```CS
string x = left?.Value;
```

Multiline version for long expressions ([Law of Demeter](https://en.wikipedia.org/wiki/Law_of_Demeter)):

```CS
string x = left
    ?.Property1
    ?.Property2
    ?.Property3
    ?.Property4
    ;
```

## Chaining
When chaining members, do so in the way that each part is in new line.

Use:
```CS
List<string> nums = Enumerable.Range(0, 100)
    .Where(x => x > 50)
    .Select(x => x.ToString())
    .OrderBy(x => x)
    .ToList();
```

Instead of:
```CS
List<string> nums = Enumerable.Range(0, 100).Where(x => x > 50).Select(x => x.ToString()).OrderBy(x => x).ToList();
```

# Exceptions
* Don't throw inside try catch;
* Rethrow caught exceptions instead of creating new if you don't need new object;
* If you need new exception then pass caught exception to internal exception of new exception;

# Events
* Always subscribe with a method;
* Always unsubscribe;

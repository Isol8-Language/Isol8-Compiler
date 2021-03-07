<!-- omit in toc -->
# ISOL8 Test Cases and Examples

<!-- omit in toc -->
## Table of Contents
- [1. Declaring Variables](#1-declaring-variables)
- [2. Maths Operations](#2-maths-operations)
- [3. Input/Output Operations](#3-inputoutput-operations)
- [4. Pointers](#4-pointers)
<br>
<br>

## 1. Declaring Variables
| Test Case | Code Block | Expected Result | Actual Result | Pass? (Y/N) |
|---|---|---|---|:---:|
|Null integer variable|[1.1](#11-code-block)|Compiles successfully|<-|Y|
|Integer with maximum signed value (+2^31 -1)|[1.2](#12-code-block)|Compiles successfully|<-|Y|
|Integer with minimum signed value (-2^31)|[1.3](#13-code-block)|Compiles successfully|<-|Y|
|Integer with value over maximum signed 32bit value|[1.4](#14-code-block)|?|Compiler error: TYPE_MISMATCH|?|
|Integer with value over minimum signed 32bit value|[1.5](#15-code-block)|?|Compiles successfully|?|
|Empty string|[1.6](#16-code-block)|Compiles successfully|Compiler error: A2047|N|
|String containing data|[1.7](#17-code-block)|Compiles successfully|<-|Y|
|Creating a null pointer|[1.8](#18-codeb-block)|Compiles successfully|<-|Y|
All code blocks have the ``Initial()`` method redacted to improve readability, unless stated otherwise.
<!-- omit in toc -->
### 1.1 Code Block
```
testVar as int null;
```
<!-- omit in toc -->
### 1.2 Code Block
```
testVar as int 2147483647;
```
<!-- omit in toc -->
### 1.3 Code Block
```
testVar as int -2147483648;
```
<!-- omit in toc -->
### 1.4 Code Block
```
testVar as int 2147483648;
```
<!-- omit in toc -->
### 1.5 Code Block
```
testVar as int -2147483649;
```
<!-- omit in toc -->
### 1.6 Code Block
```
testVar as string "";
```
<!-- omit in toc -->
### 1.7 Code Block
```
testVar as string "Hello World";
```
<!-- omit in toc -->
### 1.8 Code Block
```
testVar as ptr null;
```

<br>
<br>

## 2. Maths Operations
| Test Case | Code Block | Expected Result | Actual Result | Pass? (Y/N) |
|---|---|---|---|:---:|
||||||
||||||
||||||

<br>
<br>

## 3. Input/Output Operations
| Test Case | Code Block | Expected Result | Actual Result | Pass? (Y/N) |
|---|---|---|---|:---:|
|Printing null integer to console|[2.1](#21-code-block)|Compiles successfully, prints 0 to console|<-|Y|
|Printing integer with max signed 32bit value|[2.2](#22-code-block)|Compiles successfully, prints correct value|<-|Y|
|Printing integer with min signed 32bit value|[2.3](#23-code-block)|Compiles successfully, prints correct value|<-|Y|
|Printing integer with value over max signed 32bit value|[2.4](#24-code-block)|?|Compiler Error: TYPE_MISMATCH|?|
|Printing integer with value over minimum signed 32bit value|[2.5](#24-code-block)|?|Compiler Error: TYPE_MISMATCH|?|
|Printing empty string|[2.5](#25-code-block)|Compiles successfully, prints nothing to console|Compiler error due to [1.6](#16-code-block)|N|
|Printing string with content|[2.6](#26-code-block)|Compiles successfully, prints content of string to console|<-|Y|
<br>
<br>

<!-- omit in toc -->
### 2.1 Code Block
```
a as int null;
Initial() ret int
{
	out(a);
	ret 0;
}
```
<!-- omit in toc -->
### 2.2 Code Block
```
a as int 2147483647;
Initial() ret int
{
	out(a);
	ret 0;
}
```
<!-- omit in toc -->
### 2.3 Code Block
```
a as int -2147483648;
Initial() ret int
{
	out(a);
	ret 0;
}
```
<!-- omit in toc -->
### 2.4 Code Block
```
a as int 2147483648;
Initial() ret int
{
	out(a);
	ret 0;
}
```
<!-- omit in toc -->
### 2.5 Code Block
```
a as int -2147483649;
Initial() ret int
{
	out(a);
	ret 0;
}
```

## 4. Pointers
| Test Case | Code Block | Expected Result | Actual Result | Pass? (Y/N) |
|---|---|---|---|:---:|
||||||
||||||
||||||

<br>
<br>

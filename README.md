# Legion keyboard RGB
### Current version: 0.1.0

### Mirrors the content on the screen with an option to apply colour correction 
---

### **Launch parameters:**
- sat - Set custom saturation level
- light - Set custom lightness level
- fps - Set custom FPS value (default: 20 FPS)

**Parameters are written in this format:** `<parameter_name>=<value>`
Parameter value can be a whole number or a decimal number written in the system locale.

**Accepted ranges for each parameter value:**
- Saturation and Lightness: 0.1 - 10\*
- FPS: 1-100\*\*

_\*Values over 2 are effective on dark colours only. Saturation and lightness cannot exceed 100%_ 

_\*\*The current implementation of capturing the screen doesn't allow update rates higher than 25-30 FPS without frequent frame drops. Going Above 30FPS may (or will) result in frequent and/or large frame drops_


**The text colour changes if the app is lagging behind the desired frame-rate:**

- 1-10ms behind: _Yellow_
- 10ms+ behind: _Red_

---
**Code for controlling the backlight is from [4JX/L5P-Keyboard-RGB](https://github.com/4JX/L5P-Keyboard-RGB)** /driver/src directory. Compiled into a DLL for an easy acces from C#.

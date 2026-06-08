# Architecture

LaserCursor is organized around a small shared app state, a global hotkey manager, a transparent overlay window, a mouse tracker, and a stroke renderer.

The first implementation uses WPF drawing primitives for speed of development. Rendering is kept separate from the drawing state so a later Direct2D renderer can replace the WPF renderer without changing hotkeys, cursor tracking, or stroke lifetime logic.

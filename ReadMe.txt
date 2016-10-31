README
AMIRREZA SAMIEI
HOMEWORK 1
============================================================================================================
============================================================================================================

Execution:
For executing the program bin\Debug\HW1Armin.exe can be run.
============================================================================================================

Manual:
Any part of the canvas that is not covered by UI controls can be written on.
To load a background the Background button could be pressed and any image would be loaded to the background.
To change the colour of the background the colour-picker scroll down can be pressed to view list of colours available.
The Save and Load operations are working as described in the requirements.
The slider bar is to be used for changing the tip size of the stylus and the colour picker next to this 
slider is for selecting the colour of the tip of the stylus.
The modes in the box on the left are for erasing and selecting using lasso and inking.
The same Select mode can be used to lasso a set of strokes and the recognized text would be shown on the 
bottom of the screen in a text area.

The Repeat button opens a new window which contains the same controls for stylus settings. The New button
causes recording a new gesture set. Anything drawn before initializing would not be saved. Afterwards when
the user is finished with the drawing it can save it. The save is incremental and until a new set is began
it can be continued drawing and saving.

==========================================================================================================

How it was done:
It was a pretty straight forward homework and everything was already implemented and 
nothing needed not to be done for most of the requirements. The only difficult part was 
the recognition part which strokes had to be passed in to the recognizer using MemoryStream since most 
of the solutions on the net were for a Tablet PC. The Microsoft.Ink dll was used for recognition. 
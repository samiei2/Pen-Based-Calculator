README
AMIRREZA SAMIEI
HOMEWORK 3
============================================================================================================
============================================================================================================

Execution:
For executing the program bin\Debug\HW1Armin.exe can be run.
============================================================================================================

How it was done:
I used the implementation of $N described in the paper and their source code available online to implement
and integrate the $N recognizer into my project. They had the implemented both GSS and Protractor measurement
inside their code. I implemented the Penny Pincher code from the suedocode in the paper and a swift implementation
of the code I found online. Then I changed the repeat window I implemented in howework 1 to be compatible 
with the current code because of the datasets we needed. After implementations were finished I recorded 5
repeatitions per gesture and then at runtime loaded 1,3 or 5 number of samples for recognition.
Both $N and Protractor worked fine and except some recognition problems for gesture like 5 and S or 6 and B
the rest were working almost perfectly though we had some misclassifications from time to time.
The Penny Pincher code seems not working and although I have checked it line by line I still can't figure out
whats wrong with the code because it misclassifies almost every time. I'm still looking into the problem but
can't figure out what to do yet.
For the segmentation I used a time based segmentation with a 1000 sec window for drawing the gestures and
afterwards the recognizer kicks in.

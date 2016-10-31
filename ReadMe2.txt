README
AMIRREZA SAMIEI
HOMEWORK 2
============================================================================================================
============================================================================================================

Execution:
For executing the program bin\Debug\HW1Armin.exe can be run.
============================================================================================================

How it was done:

For the implementation of shape recognition I used the IStraw ,PaleoSketch and domain-independent system
for skecth recognition papers. The brief implementation is as follows. First I removed duplicate points from
a stroke but I didn't use any timing factor for removing points just coordinates. The second was calculation
of direction and curvature graph like the ones described in the PaleoSketch paper. These are arrays
in my implementation containing values for each point. Then I removed the hooks from the stroke if it had any.
Again I used the PaleoSketch instructions regarding detecting hooks which looked for high curvature in the first
and last 20 percent of the stroke. Then I calculated some extra features that the paper used for better recognition,
like calculating NDDE and DCR, etc. After computing the features first I tried detecting the straight simple lines 
using the criteria described in the paper. Then I went on implementing the poly line detectiong and number of lines
constructing the polyline. For this part I had to have corner detection to break the stroke into parts and check their
line property. For the corner detection I used IStraw algorithm although since it was kinda vague in the implementation
I also had to use some parts of ShortStraw to better understand it. Probably one of the hard ones implementing is the 
detection of the ellipse and circles. Though with a good heuristic could be detected most of the times if drawn correctly.
For ellipse the I had to compute major and minor axis to find the difference between circle and ellipse which was easy with
just finding the furthest points and intersect a line perpendicular to the major axis with the ellipse to find the minor axis.
For multiple touch manipulation the implementation was pretty easy since we only had to apply the manipulation delta to the
selected stroke as a transform matrix. And the grouping is done by selecting multiple strokes and adding them to a single
stroke which connects the the points together.





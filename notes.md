# Just some notes
## Need to improve burn word algorithm
Current (2023/04/10) output of `dotnet run -- -i caremnchlt -c s5-u2-s3-y5+`:
```
3 Possible words:
pousy: 14
soupy: 14
bousy: 13

Letter frequency:
o: 3, s: 3, u: 3, y: 3, p: 2, b: 1, a: 0, c: 0, d: 0, e: 0, f: 0, g: 0, h: 0, i: 0, j: 0, k: 0, l: 0, m: 0, n: 0, q: 0, r: 0, t: 0, v: 0, w: 0, x: 0, z: 0, 

Letter frequency by position:
Position 1: b: 1, p: 1, s: 1, 
Position 2: o: 3, 
Position 3: u: 3, 
Position 4: s: 2, p: 1, 
Position 5: y: 3, 

First 10 burn words:
jowpy: 7
podgy: 7
gowdy: 6
wodgy: 6
bawdy: 4
bodge: 4
bovid: 4
bovld: 4
bowed: 4
bowge: 4
```
Ideal burn word would be `blips` as it takes into account the most common letters in the 3 possible words.

Also, there's no point in suggesting so many burn words for when there are only 3 possible solutions.

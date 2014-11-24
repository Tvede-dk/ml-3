from os.path import isfile, join

genome = [ join('..', 'hhm', 'genome', 'genome1.fa')
         , join('..', 'hhm', 'genome', 'genome2.fa')
         , join('..', 'hhm', 'genome', 'genome3.fa')
         , join('..', 'hhm', 'genome', 'genome4.fa')
         , join('..', 'hhm', 'genome', 'genome5.fa')]
for g in genome:
  # Check that the files exists
  assert isfile(g)
  
annotation = [ join('..', 'hhm', 'annotation', 'annotation1.fa')
             , join('..', 'hhm', 'annotation', 'annotation2.fa')
             , join('..', 'hhm', 'annotation', 'annotation3.fa')
             , join('..', 'hhm', 'annotation', 'annotation4.fa')
             , join('..', 'hhm', 'annotation', 'annotation5.fa')]
for a in annotation:
  # Check that the files exists
  assert isfile(a)

start = dict()
stop = dict()
revStart = dict()
revStop = dict()

for i in xrange(len(genome)):
  # Read the file and skip the first "information" line
  with open(genome[i], 'rb') as gen:
    g = ''.join([x.strip().upper() for x in gen.readlines()[1:]])
  with open(annotation[i], 'rb') as ann:
    a = ''.join([x.strip().upper() for x in ann.readlines()[1:]])
  # Ensure that the files are of equal length  
  assert len(g) == len(a)
  
  # Assume that all genomes starts as non-coding
  assert a[0] == 'N'
  coding = False
  for j in xrange(len(g)):
    if not coding and a[j] == 'C':
      codon = g[j] + g[j+1] + g[j+2]
      start[codon] = start.get(codon, 0) + 1
      j = j + 2
      coding = True
    elif not coding and a[j] == 'R':
      codon = g[j] + g[j+1] + g[j+2]
      revStart[codon] = revStart.get(codon, 0) + 1
      j = j + 2
      coding = True   
    elif coding and a[j] == 'N':
      codon = g[j-3] + g[j-2] + g[j-1]
      coding = False
      if a[j-1] == 'C':
        stop[codon] = stop.get(codon, 0) + 1
      else:
        revStop[codon] = revStop.get(codon, 0) + 1

  # Ensure that all coding and non-coding are divisible by 3
  # And that they contains more than just stop and start
  # codon
  anno = a.split('N')
  for a in anno:
    if a == '':
      continue
    assert len(a) % 3 == 0
    assert len(a) > 6

# Print the final result
print 'N -> C (START)'
print '=============='
for codon in start:
  print codon, start[codon]
  
print ""
print 'C -> N (STOP)'
print '============='
for codon in stop:
  print codon, stop[codon]

print ''
print 'N -> R (START)'
print '=============='
for codon in revStart:
  print codon, revStart[codon]
  
print ''
print 'R -> N (STOP)'
print '=============='
for codon in revStop:
  print codon, revStop[codon]

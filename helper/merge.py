import sys
from os.path import isfile

try:
	# Check user input
	assert len(sys.argv) == 3
	assert isfile(sys.argv[1])
	assert isfile(sys.argv[2])
except AssertionError:
	print 'Usage: python merge.py [C-only file] [R-only file]'
	sys.exit(1)

with open(sys.argv[1], 'rb') as cFile, open(sys.argv[2], 'rb') as rFile:
	cAnn = cFile.readlines()
	rAnn = rFile.readlines()
	# Annotation came from the same file
	if cAnn[0] != rAnn[0]:
		print 'C-only and R-only file must origin from the same genome'
		sys.exit(1)
	firstLine = cAnn[0]
	del cAnn[0]
	del rAnn[0]
	
	cAnn = ''.join([c.strip() for c in cAnn])
	rAnn = ''.join([r.strip() for r in rAnn])
	assert len(cAnn) == len(rAnn)

ann = [firstLine]
tmp = ''
for i in xrange(len(cAnn)):
	if len(tmp) == 60:
		ann.append(tmp + '\r\n')
		tmp = ''
		
	if cAnn[i] == 'C':
		tmp = tmp + 'C'
	elif rAnn[i] == 'R':
		result = tmp + 'R'
	else:
		tmp = tmp + 'N'
	
with open('merge.txt', 'wb') as out:
	out.write(''.join(ann))

import sys

with open(sys.argv[1], 'rb') as cFile, open(sys.argv[2], 'rb') as rFile:
	cAnn = cFile.readlines()
	rAnn = rFile.readlines()
	# Annotation came from the same file
	assert cAnn[0] == rAnn[0]
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

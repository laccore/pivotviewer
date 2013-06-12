#!/bin/bash

#################################################################
# package.sh
# Brian Grivna
# June 7, 2013
# 
# Script to add low-resolution images, metadata CSV file to
# the TMI PivotViewer XAP package.
#################################################################

# save off original IFS, so we can restore at end of script
SAVEIFS=$IFS 

# $IFS contains delimiters that determine how fields in for
# loops are interpreted: by default, it includes a space, which
# creates problems when filenames contain spaces. I believe TMI
# isn't techically supposed to allow spaces in image filenames,
# but one slipped through the cracks. Temporarily removing space
# from the delimiters gets the job done.
IFS=$(echo -en "\n\b")

COMPONENTS_DIR=/Users/bgrivna/Desktop/components
PACKAGE_DIR=/Users/bgrivna/Desktop/pivotImages
XAP_DIR=/Users/bgrivna/Desktop
XAP_FILE=TmiPivotDevelopment
CSV_PATH=/Users/bgrivna/Desktop/TmiPivotMetadata.csv

mkdir $PACKAGE_DIR
mkdir $PACKAGE_DIR/components

cp $XAP_DIR/$XAP_FILE.xap $PACKAGE_DIR/$XAP_FILE.zip

unzip $PACKAGE_DIR/$XAP_FILE.zip -d $PACKAGE_DIR

cd $COMPONENTS_DIR

for IMAGE in `find . | grep "200s"`
do
	rsync -Rv $IMAGE $PACKAGE_DIR/components # -R preserves dir structure
done

cd $PACKAGE_DIR

cp $CSV_PATH .

#rm -r __MACOSX # probably not necessary in prod
rm $XAP_FILE.zip

zip -r $XAP_FILE.xap *

# todo: move XAP to deployment location, cleanup PACKAGE_DIR

# restore original IFS
IFS=$SAVEIFS
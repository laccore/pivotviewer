#!/bin/bash

#################################################################
# package.sh
# Brian Grivna
# June 7, 2013
#
# Script to add low-resolution images, metadata CSV file to
# the TMI PivotViewer XAP package.
#
# Reed McEwan
# July 2, 2013
# Modified the staging dir structures to standardize and simplify
# 
# Staging Dir Structure:
#	staging_dir (grails config setting)
#		script	(holds this packaging script written from the db)
#		resources (holds the empty xap written from the db and the generated metadata CSV)
#		logs (holds stout and sterr output from last xap refresh
#		deployment (holds the latest copy of the full xap, streamed live from this location)	
# 
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

echo "begin $(date)"

STAGING_DIR=$1					#/Users/rmcewan/pivot
COMPONENTS_DIR=$2				#/Users/rmcewan/Desktop/TMI/dev-images (grails config setting)
XAP_FILE=$3                      #TmiPivotDevelopment
CSV_FILE=$4                      #TmiPivotMetadata.csv


mkdir $STAGING_DIR/working    
mkdir $STAGING_DIR/working/components

cp $STAGING_DIR/resources/$XAP_FILE.xap $STAGING_DIR/working/$XAP_FILE.zip #cp to working dir
cp $STAGING_DIR/resources/$CSV_FILE.csv . #cp to working dir

unzip $STAGING_DIR/working/$XAP_FILE.zip -d $STAGING_DIR/working #unzip to working dir

cd $COMPONENTS_DIR

#count=1
for IMAGE in `find . | grep "200s"`
do
rsync -R $IMAGE $STAGING_DIR/working/components # -R preserves dir structure
#if [ "$count" -gt 150 ]
#then
#break
#fi
#count=$(($count+1))
done


cd $STAGING_DIR/working

cp $STAGING_DIR/resources/$CSV_FILE.csv . #cp to working dir, not necessary b/c written from db to package_dir

#rm -r __MACOSX # probably not necessary in prod
rm $XAP_FILE.zip

zip -r $XAP_FILE.xap *

# todo: move XAP to deployment location, cleanup PACKAGE_DIR
mv $XAP_FILE.xap $STAGING_DIR/deployment

#clean up
rm -R $STAGING_DIR/working
rm -R "just some output to sterr"	#just to write something to sterr

echo "end $(date)"

# restore original IFS
IFS=$SAVEIFS
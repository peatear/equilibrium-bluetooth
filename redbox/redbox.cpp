// redbox.cpp
#include "pch.h"
#include "redbox.h"
#include <stdlib.h>

using namespace redbox;
using namespace Platform;

Redbox::Redbox(int width, int height, const Platform::Array<int>^ markerdata)
{
	minWidth = width;
	minHeight = height;
	this->markerptr = (uintptr_t)markerdata->Data;
	marker = Frame(width, height, markerptr);
}


void Redbox::drawMarker(int x, int y){
	for (int ydy = 0; ydy < minHeight; ydy++){
		for (int xdx = 0; xdx < minWidth; xdx++){
			f(x + xdx, y + ydy) = marker(xdx, ydy);
		}
	}

}

bool Redbox::checkRed(int x, int y){
	//return f(x, y).red>5 * (f(x, y).blue + f(x, y).green);
	//return f(x,y).red>90&&f(x,y).blue<90&&f(x,y).green<90;
	return (f(x, y).red - (f(x, y).red*0.2989 + f(x, y).green*0.5870 + f(x, y).blue*0.1140))>50;
}

void Redbox::manipVid(unsigned int width, unsigned int height, uintptr_t dataPtr){
	f = Frame(width, height, dataPtr);


	randsamp = 50;
	x = 0;
	y = 0;
	px = 0;
	nx = 0;
	py = 0;
	ny = 0;

	
	
	for (int iter = 0; iter < randsamp; iter++){
		x = rand() % (width-2*minWidth)+minWidth;
		y = rand() % (height-2*minHeight)+minHeight;

		

		if (checkRed(x,y)){

			px = x;
			py = y;
			nx = x;
			ny = y;

			while (checkRed(px,y)&&px<width-minWidth){
				px+=2;

			}
			while (checkRed(nx, y)&&nx>minWidth){
				nx-=2;
			}

			
			
			while (checkRed(x, py) && py<height-minHeight){
				py+=2;

			}
			while (checkRed(x, ny) && ny>minHeight){
				ny-=2;
			}
			
			if (px - nx > minWidth&&py - ny > minHeight){

				y = ny + ((py - ny) / 2-minHeight/2);
				x = nx + ((px - nx) / 2-minWidth/2);
				drawMarker(x, y);
			}
		
			
		}
		
		
	}
	
	this->onFrameReady(f.width(), f.height(), f.data());
}
#pragma once
#include "Frame.h"

namespace redbox
{
	public delegate void onFrameReadyEvent(unsigned int width, unsigned int height, uintptr_t dataPtr);
    public ref class Redbox sealed
    {

	private:
		Frame marker;
		Frame f;
		uintptr_t markerptr;
		int randsamp;
		int minHeight;
		int minWidth;
		int x;
		int y;
		int px;
		int nx;
		int py;
		int ny;

    public:

		bool Redbox::checkRed(int x, int y);

		void Redbox::drawMarker(int x, int y);

		Redbox(int width, int height, const Platform::Array<int>^ markerdata);

		void manipVid(unsigned int width, unsigned int height, uintptr_t dataPtr);

		event onFrameReadyEvent^ onFrameReady;
    };
}
#pragma once

struct Window {
	char* className;
	char* text;
};

typedef struct {
	double cx;
	double cy;
} DSIZE;

typedef struct {
	HWND hwnd;
	POINT temp;

	bool operator==(const HWND& hwnd) {
		return this->hwnd == hwnd;
	}
} Movement;

typedef struct {
	unsigned int keyflag;
	unsigned int keycode;
} Key;

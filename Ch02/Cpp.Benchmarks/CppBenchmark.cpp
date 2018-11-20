// hayai - the C++ benchmarking framework.
// https://github.com/nickbruun/hayai
//
// Alternatives:
// http://www.bfilipek.com/2016/01/micro-benchmarking-libraries-for-c.html
// https://nonius.io/authoring-benchmarks

#include "stdafx.h"

#include "hayai-master\src\hayai.hpp"
#pragma warning(disable:4996)
#define _CRT_SECURE_NO_WARNINGS
#include "hayai-master\src\hayai_main.hpp"

BENCHMARK(AccessPattern, IJ, 10, 10)
{
	int n = 5000;
	int m = 5000;
	int* tab = new int[n * m];
	for (int j = 0; j < m; ++j)
	{
		for (int i = 0; i < n; ++i)
		{
			tab[i + j * n] = 1;
		}
	}
	delete[] tab;
}

BENCHMARK(AccessPattern, JI, 10, 10)
{
	int n = 5000;
	int m = 5000;
	int* tab = new int[n * m];
	for (int i = 0; i < n; ++i)
	{
		for (int j = 0; j < m; ++j)
		{
			tab[i + j * m] = 1;
		}
	}
	delete[] tab;
}

int main(int argc, char** argv)
{
	// Set up the main runner.
	hayai::MainRunner runner;

	// Parse the arguments.
	int result = runner.ParseArgs(argc, argv);
	if (result)
		return result;

	// Execute based on the selected mode.
	return runner.Run();
}


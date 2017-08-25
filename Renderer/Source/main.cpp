// Renderer.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "argparse.hpp"
#include "stdlib.h"
#include <string>
using namespace std;

int main(int argc, const char* argv[])
{
	ArgumentParser parser;
	parser.addArgument("--ip",1);
	parser.addArgument("--host",1);

	parser.parse(argc, argv);
	string ip = parser.retrieve<string>("ip");





	system("pause");
    return 0;
}


#include "FileManager.h"

FileManager::FileManager()
{
}

FileManager::~FileManager()
{
}

void FileManager::save(char* filePath, float* data, int numObjs, int stride)
{
	ofstream file(filePath, ios::out, ios::binary);
	streamsize floatSize = sizeof(float);
	int offset = 0;

	if (file.is_open())
	{
		file.write(reinterpret_cast<char*>(&numObjs), sizeof(int));

		for (int i = 0; i < numObjs; ++i)
		{
			file.write(reinterpret_cast<char*>(&data[offset]), floatSize);
			file.write(reinterpret_cast<char*>(&data[offset + 1]), floatSize);
			file.write(reinterpret_cast<char*>(&data[offset + 2]), floatSize);
			file.write(reinterpret_cast<char*>(&data[offset + 3]), floatSize);

			offset += stride;
		}

		//file.write(reinterpret_cast<char*>(&data), sizeof(float) * numObjs * stride);

		file.close();
	}

	//ofstream file(filePath, ios::out);
	//streamsize floatSize = sizeof(float);
	//int offset = 0;

	//if (file.is_open())
	//{
	//	file << numObjs << "\n";

	//	for (size_t i = 0; i < numObjs; ++i)
	//	{
	//		file << data[offset] << " " << data[offset + 1] << " " << data[offset + 2] << " " << data[offset + 3] << "\n";
	//		offset += stride;
	//	}

	//	file.close();
	//}
}

void FileManager::load(char* filePath, int stride)
{
	ifstream file(filePath, ios::in, ios::binary);
	int size = 0;
	int offset = 0;

	if (file.is_open())
	{
		file.read(reinterpret_cast<char*>(&_numObjs), sizeof(int));

		size = _numObjs * stride;
		_data = new float[size];
		for (int i = 0; i < _numObjs; ++i)
		{
			file.read(reinterpret_cast<char*>(&_data[offset]), sizeof(float));
			file.read(reinterpret_cast<char*>(&_data[offset + 1]), sizeof(float));
			file.read(reinterpret_cast<char*>(&_data[offset + 2]), sizeof(float));
			file.read(reinterpret_cast<char*>(&_data[offset + 3]), sizeof(float));

			offset += stride;
		}

		//size = _numObjs * stride;
		//_data = new float[size];
		//file.read(reinterpret_cast<char*>(&_data), size);

		file.close();
	}
}

float* FileManager::getData() const
{
	return _data;
}

int FileManager::getNumObjs() const
{
	return _numObjs;
}

void FileManager::logMetrics(char* filePath, int kills, float accuracy, int adrenCounter)
{
	ofstream file(filePath, ios::out);

	if (file.is_open())
	{
		file << "Kills: " << kills << std::endl;
		file << "Accuracy: " << accuracy << std::endl;
		file << "Number of times Adrenaline Rush was used: " << adrenCounter << std::endl;

		file.close();
	}
}

using System.Net;
using System;
using UnityEngine;
using System.Text;
using Unity.Networking.Transport;
using Unity.Collections;
using System.Collections.Generic;
using System.Collections;

public static class Conversions
{
    //Var to Bytes
    public static byte[] StringToBytes(string x) {
        byte[] bytes = Encoding.ASCII.GetBytes(x);
        return bytes;
    }

    public static byte[] VectorAxisToBytes(float axis) {
        double rounded = Conversions.Round(axis, 2);
        byte[] bytes = BitConverter.GetBytes(rounded);
        return bytes;
    }

    public static byte[] FloatToBytes(float x) {
        byte[] bytes = BitConverter.GetBytes(x);
        return bytes;
    }

    //Bytes to Var
    public static string BytesToString(byte[] bytes) {
        string x = Encoding.Default.GetString(bytes);
        return x;
    }

    public static double BytesToDouble(byte[] bytes) {
        double x = System.BitConverter.ToDouble(bytes, 0);
        return x;
    }

    public static Vector2 BytesToVector2(byte[] bytesX, byte[] bytesZ) {

        double x = System.BitConverter.ToDouble(bytesX, 0);
        double z = System.BitConverter.ToDouble(bytesZ, 0);

        Vector2 vector = new Vector2((float)x, (float)z);
        return vector;
    }
   
    //OTHER MATH FUNCTIONS-----------------
    //Round float up to given decimals
    public static double Round(float value, int digits) {
        float mult = Mathf.Pow(10.0f, (float)digits);
        return (double)Mathf.Round(value * mult) / mult;
    }

    //Calculate a line between two vector points
    public static Vector2 CalculateLine(Vector2 x1, Vector2 x2, double distance) {
        //calculate length with Pythagoras
        double length = Math.Sqrt(multiply(x2.x - x1.x) + multiply((x2.y - x1.y)));
        double unitSlopeX = (x2.x - x1.x) / length;
        double unitSlopeY = (x2.y - x1.y) / length;
        double x = x1.x + unitSlopeX * distance;
        double y = x1.y + unitSlopeY * distance;
        return new Vector2((float)x, (float)y);
    }


    //multiply any number
    public static double multiply(double one) {
        return one * one;
    }

    //convert int to time
    public static string toTime(int _time){
        string timeString = "";
        int minutes = 0;
        while(_time > 59){
            minutes++;
            _time -= 60;
        }
        int seconds = _time;
        
        timeString = minutes + ":" + seconds;
        return timeString;
    }

}

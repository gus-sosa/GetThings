""" Created by Henrikh Kantuni and Shahen Kosyan on 4/21/16. """

from bs4 import BeautifulSoup
from random import randrange
from time import sleep
import requests
import json
import os
import re

USERNAME = ''  # enter your Pluralsight account username (or email)
PASSWORD = ''  # enter your Pluralsight account password

WORKING_DIRECTORY = os.getcwd()  # save current working directory
ALL_COURSES_FOLDER_NAME = 'Courses'


def create_all_courses_folder():
    """
    Create a folder for all downloaded courses

    :return: None
    """
    global WORKING_DIRECTORY, ALL_COURSES_FOLDER_NAME

    os.chdir(WORKING_DIRECTORY)
    if not os.path.exists(ALL_COURSES_FOLDER_NAME):
        os.mkdir(ALL_COURSES_FOLDER_NAME)


def create_course_folder(course_name):
    """
    Create a folder for a new course

    :param course_name: a new course folder name
    :return: None
    """
    global WORKING_DIRECTORY, ALL_COURSES_FOLDER_NAME

    os.chdir(WORKING_DIRECTORY + '/' + ALL_COURSES_FOLDER_NAME + '/')
    if not os.path.exists(course_name):
        os.mkdir(course_name)


def create_module_folder(course_name, module_name):
    """
    Create a module folder in a course folder

    :param course_name: a course name
    :param module_name: a module name
    :return: None
    """
    global WORKING_DIRECTORY, ALL_COURSES_FOLDER_NAME

    os.chdir(WORKING_DIRECTORY + '/' + ALL_COURSES_FOLDER_NAME + '/' + course_name)
    if not os.path.exists(module_name):
        os.mkdir(module_name)


def rename_file(old_name, new_name):
    """
    Rename a file

    :param old_name: old name of the file
    :param new_name: new name of the file
    :return: None
    """
    # replace unacceptable symbols
    bad_chars = '[^\w\-_\. ]'
    new_name = re.sub(bad_chars, '', new_name)
    os.rename(old_name, new_name)


def download_via_url(course_name, module_name, clip_name, clip_url, clip_index):
    """
    1. Create all courses folder
    2. Create a course folder in all courses folder
    3. Create a module folder in the corresponding course folder
    4. Download a file from the given URL
    5. Rename the file

    :param course_name: a course folder name
    :param module_name: a module folder name
    :param clip_name: new name of the file
    :param clip_url: URL to download
    :param clip_index: order
    :return: None
    """
    global WORKING_DIRECTORY, ALL_COURSES_FOLDER_NAME

    create_all_courses_folder()
    create_course_folder(course_name)
    create_module_folder(course_name, module_name)
    os.chdir(WORKING_DIRECTORY + '/' + ALL_COURSES_FOLDER_NAME + '/' + course_name + '/' + module_name)
    clip_name += '.mp4'

    # get the video file name from the url
    old_name = ''
    for chars in clip_url.split('/'):
        if '.mp4' in chars:
            arguments = chars.split('?')
            for video in arguments:
                if '.mp4' in video:
                    old_name = video

    # replace unacceptable symbols
    bad_chars = '[^\w\-_\. ]'
    clip_name = re.sub(bad_chars, '', clip_name)
    clip_name = '{0} - {1}'.format(clip_index, clip_name)

    if not os.path.exists(clip_name):
        print("Downloading " + clip_name)
        response = requests.get(clip_url)
        sleep(randrange(7, 15))  # be nice
        with open(old_name, 'wb') as file:
            file.write(response.content)
        rename_file(old_name, clip_name)
    else:
        print("File " + clip_name + " already exists.")


def make_soup(url):
    """
    Return a BeautifulSoup instance to parse as lxml

    :param url: URL
    :return: BeautifulSoup instance
    """
    page = requests.get(url)
    sleep(randrange(7, 15))  # be nice
    html = page.content
    return BeautifulSoup(html, 'lxml')


def login(username, password):
    """
    Login to Pluralsight using your username (or email) and password

    :param username: account username (or email)
    :param password: account password
    :return: session
    """
    if not(username and password):
        exit("Please fill in your Pluralsight username (or email) and password at the top of the file.")

    login_url = 'https://app.pluralsight.com/id/'
    headers = {
        'User-Agent': 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_4) '
                      'AppleWebKit/601.5.17 (KHTML, like Gecko) Version/9.1 Safari/601.5.17'
    }
    data = {
        'RedirectUrl': '',
        'Username': username,
        'Password': password,
        'ShowCaptcha': 'False',
        'ReCaptchaSiteKey': '6LeVIgoTAAAAAIhx_TOwDWIXecbvzcWyjQDbXsaV'
    }

    with requests.Session() as session:
        session.post(login_url, headers=headers, data=data)
        sleep(randrange(7, 15))  # be nice
        return session


def download_course(url):
    """
    Download a course from the given URL

    :param url: a course URL
    :return: None
    """
    global USERNAME, PASSWORD

    with login(USERNAME, PASSWORD) as session:
        html = make_soup(url)

        course_title = html.find(id='course-hero-container').find('h2').getText().strip()
        course_title = ' '.join(course_title.split(' '))  # trim
        course_author = html.find(id='course-hero-container').find('h5').find('a').getText().strip()
        course_author = ' '.join(course_author.split(' '))  # trim
        course_title = course_title + ' - ' + course_author

        print('\n' + course_title + '\n')

        json_url = url.replace('library', 'learner/content').replace('/table-of-contents', '')
        course = json.loads(session.get(json_url).text)
        sleep(randrange(7, 15))  # be nice
        for module_index, module in enumerate(course['modules'], start=1):
            module_title = '{0} - {1}'.format(module_index, module['title'])
            for clip_index, clip in enumerate(module['clips'], start=1):
                clip_title = clip['title']
                retrieve_url = 'https://app.pluralsight.com/video/clips/viewclip'
                request_data = clip['id'].split('|')
                course_name = request_data[0]
                author_name = request_data[1]
                module_name = request_data[2]
                video_resolutions = ['1024x768', '1280x720']  # in this order
                for resolution in video_resolutions:
                    request = {
                        'author': author_name,
                        'includeCaptions': False,
                        'clipIndex': clip_index - 1,
                        'courseName': course_name,
                        'locale': 'en',
                        'moduleName': module_name,
                        'mediaType': 'mp4',
                        'quality': resolution
                    }

                    while True:
                        try:
                            clip_url = json.loads(session.post(retrieve_url, json=request).text)
                            sleep(randrange(7, 15))  # be nice
                            break
                        except requests.exceptions.ReadTimeout:
                            pass

                    if 'status' in clip_url:
                        if clip_url['status'] == 404:
                            continue
                        elif clip_url['status'] == 403:
                            print(clip_url)
                            raise ValueError("Pluralsight has blocked your account.")
                        else:
                            print(clip_url)
                            raise ValueError("Unknown status code.")
                    elif 'urls' in clip_url:
                        clip_url = clip_url['urls'][0]['url']
                    else:
                        print(clip_url)
                        raise ValueError("Unknown response.")

                    # download and save a video
                    download_via_url(course_title, module_title, clip_title, clip_url, clip_index)
                    break


def download_all_courses(file_name):
    """
    Download all courses from the given file

    :param file_name: file with URLs of all courses to download
    :return: None
    """
    with open(file_name, 'r') as file:
        for url in file.read().strip().split('\n'):
            download_course(url)
            sleep(randrange(30, 50))  # be nice


# WARNING: high volume of requests may temporarily block your account.
# To download multiple courses put each course URL
# on a separate line into courses.txt file.
download_all_courses('courses.txt')

# To download only one course pass course URL
# as an argument to download_course function
# download_course('url here')
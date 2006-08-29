(global-font-lock-mode 't)
(setq-default transient-mark-mode t)
(setq fill-column 80)

(global-set-key "\M-g" 'goto-line)
(global-set-key "\C-z" 'undo)

;; Put autosave files (ie #foo#) in one place, *not* scattered all over the
;; file system! (The make-autosave-file-name function is invoked to determine
;; the filename of an autosave file.)
(defvar autosave-dir "~/.Trash/")
(make-directory autosave-dir t)
(defun auto-save-file-name-p (filename) (string-match "^#.*#$" (file-name-nondirectory filename)))

(defun make-auto-save-file-name () 
  (concat autosave-dir 
	    (if buffer-file-name 
		      (concat "#" 
			            (file-name-nondirectory buffer-file-name) 
				          "#") 
	          (expand-file-name 
		        (concat "#%" (buffer-name) "#")
			     ))))

;; Put backup files (ie foo~) in one place too. (The backup-directory-alist
;; list contains regexp=>directory mappings; filenames matching a regexp are
;; backed up in the corresponding directory. Emacs will mkdir it if necessary.)
(defvar backup-dir "~/.Trash/")
(setq backup-directory-alist (list (cons "." backup-dir)))

(require 'cc-mode)
(c-add-style "kjk-c-style"
	     '("linux"
	       (indent-tabs-mode . nil)
	       (c-basic-offset . 4)
))

(setq c-default-style "kjk-c-style")
